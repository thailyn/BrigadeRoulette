using Phlebotomist.Model;
using Phlebotomist.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrigadeRouletteConsole
{
    // TODO: Use Phlebotomist.ViewModels.BrigadeVerticalPosition (and equivalent enum for
    // horizontal positions) instead of BrigadeFormationVerticalPositionType everywhere?

    // Is this class really needed?  Or could we extend BrigadeViewModel?
    public class BrigadeFormationWithFamiliars
    {
        protected PhlebotomistRepository PhlebotomistRepository { get; set; }

        private BrigadeFormation _formation;
        public BrigadeFormation Formation
        {
            get
            {
                return _formation;
            }
            set
            {
                _formation = value;
                UpdateMaxFamiliarsInPositions();
            }
        }

        public bool IncludeReserve { get; set; }

        public double WinPercent
        {
            get
            {
                return CalculateWinPercent();
            }
        }

        public int NumFamiliarsInPosition
        {
            get
            {
                int count = 0;
                foreach (var position in FamiliarsInPositions.Keys)
                {
                    count += FamiliarsInPositions[position].Count;
                }

                return count;
            }
        }

        public Dictionary<BrigadeFormationVerticalPositionType, List<FamiliarWinPercent>> FamiliarsInPositions { get; set; }
        public Dictionary<BrigadeFormationVerticalPositionType, int> MaxFamiliarsInPositions { get; set; }

        protected double CalculateWinPercent()
        {
            throw new NotImplementedException();
        }

        protected void UpdateMaxFamiliarsInPositions()
        {
            if (Formation == null)
            {
                return;
            }

            MaxFamiliarsInPositions = new Dictionary<BrigadeFormationVerticalPositionType, int>();
            foreach (var position in Formation.Positions)
            {
                if (!MaxFamiliarsInPositions.ContainsKey(position.VerticalPositionTypes))
                {
                    MaxFamiliarsInPositions[position.VerticalPositionTypes] = 0;
                }

                MaxFamiliarsInPositions[position.VerticalPositionTypes] += (IncludeReserve ? 2 : 1);
            }
        }

        public bool HasOpenSlotsInPosition(BrigadeFormationVerticalPositionType position)
        {
            int numInPosition = FamiliarsInPositions.ContainsKey(position) ? FamiliarsInPositions[position].Count : 0;
            int maxInPosition = MaxFamiliarsInPositions.ContainsKey(position) ?
                MaxFamiliarsInPositions[position] / (IncludeReserve ? 1 : 2) : 0;
            return numInPosition < maxInPosition;
            //return FamiliarsInPositions[position].Count < MaxFamiliarsInPositions[position] / (IncludeReserve ? 1 : 2);
        }

        public BrigadeFormationWithFamiliars(PhlebotomistRepository phlebotomistRepository,
            bool includeReserve = true)
        {
            PhlebotomistRepository = phlebotomistRepository;

            IncludeReserve = includeReserve;
            FamiliarsInPositions = new Dictionary<BrigadeFormationVerticalPositionType, List<FamiliarWinPercent>>();
            MaxFamiliarsInPositions = new Dictionary<BrigadeFormationVerticalPositionType, int>();

            // Initialize each potential index in the dictionaries.
            foreach (var verticalPosition in phlebotomistRepository.Context.BrigadeFormationVerticalPositionTypes)
            {
                FamiliarsInPositions[verticalPosition] = new List<FamiliarWinPercent>();
                MaxFamiliarsInPositions[verticalPosition] = 0;
            }
        }

        public BrigadeFormationWithFamiliars(BrigadeFormationWithFamiliars other)
        {
            this.PhlebotomistRepository = other.PhlebotomistRepository;

            this.IncludeReserve = other.IncludeReserve;
            this.FamiliarsInPositions = new Dictionary<BrigadeFormationVerticalPositionType, List<FamiliarWinPercent>>();
            foreach (var key in other.FamiliarsInPositions.Keys)
            {
                this.FamiliarsInPositions[key] = new List<FamiliarWinPercent>(other.FamiliarsInPositions[key]);
            }

            this.Formation = other.Formation;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(Program.GetBrigadeFormationString(PhlebotomistRepository, Formation));
            foreach (var verticalPosition in FamiliarsInPositions.Keys.OrderByDescending(x => x.DamageDealtModifier))
            {
                var familiars = FamiliarsInPositions[verticalPosition];
                output.AppendFormat("{0}: ", verticalPosition.Name);
                for (int i = 0; i < familiars.Count; i++)
                {
                    output.AppendFormat("{0} ({1})", familiars[i].FamiliarName, familiars[i].WinPercents[verticalPosition]);
                    if (i < familiars.Count - 1)
                    {
                        output.Append(", ");
                    }
                }

                output.AppendLine();
            }

            return output.ToString();
        }
    }

    // Is this class needed?  Or could we extend FamiliarViewModel?
    public class FamiliarWinPercent
    {
        public string FamiliarName { get; set; }
        public Dictionary<BrigadeFormationVerticalPositionType, int> WinPercents { get; set; }

        public FamiliarWinPercent()
        {
            WinPercents = new Dictionary<BrigadeFormationVerticalPositionType, int>();
        }
    }

    class Program
    {
        private static string _inputFileName = "input.csv";

        static List<FamiliarWinPercent> ReadInputFile(PhlebotomistRepository phlebotomistRepository,
            string fileName)
        {
            int maxFamiliars = 10;
            var winPercents = new List<FamiliarWinPercent>();
            using (var streamReader = new StreamReader(_inputFileName))
            {
                while (!streamReader.EndOfStream && winPercents.Count < maxFamiliars)
                {
                    var line = streamReader.ReadLine();
                    var columns = line.Split(',');
                    int result;
                    if (winPercents.Count == 0 && columns.All(x => !int.TryParse(x, out result)))
                    {
                        System.Console.WriteLine("Assuming initial line contains headers: '{0}'", line);
                        continue;
                    }
                    if (columns.Length != 4)
                    {
                        throw new InvalidOperationException(
                            string.Format("Row does not contain exactly four columns of data: '{0}'", line));
                    }

                    foreach (var column in columns)
                    {
                        System.Console.Write("'{0}'\t", column);
                    }
                    System.Console.WriteLine();

                    var familiarWinPercent = new FamiliarWinPercent
                    {
                        FamiliarName = columns[0]
                    };

                    int nextInputFileColumn = 1;
                    foreach(var verticalPosition in phlebotomistRepository.Context.BrigadeFormationVerticalPositionTypes.OrderByDescending(x => x.DamageDealtModifier))
                    {
                        familiarWinPercent.WinPercents[verticalPosition] = int.Parse(columns[nextInputFileColumn]);
                        nextInputFileColumn++;
                    }

                    winPercents.Add(familiarWinPercent);
                }
            }

            return winPercents;
        }

        static string GetBrigadeFormationsString(PhlebotomistRepository phlebotomistRepository,
            IQueryable<BrigadeFormation> formations)
        {
            StringBuilder output = new StringBuilder();
            foreach (var brigadeFormation in formations)
            {
                output.Append(GetBrigadeFormationString(phlebotomistRepository, brigadeFormation));
            }

            return output.ToString();
        }

        public static string GetBrigadeFormationString(PhlebotomistRepository phlebotomistRepository, BrigadeFormation formation)
        {
            StringBuilder output = new StringBuilder();
            var rows = phlebotomistRepository.Context.BrigadeFormationVerticalPositionTypes.OrderByDescending(x => x.DamageDealtModifier);
            output.AppendLine(string.Format("Brigade Formation: {0}", formation.Name));
            foreach (var row in rows)
            {
                // using the assumption that HorizontalPositionTypeId increases from left to right
                foreach (var horizontalPosition in formation.Positions.OrderBy(x => x.HorizontalPositionTypeId))
                {
                    if (horizontalPosition.VerticalPositionTypes.Id == row.Id)
                    {
                        output.Append("*");
                    }
                    else
                    {
                        output.Append(" ");
                    }
                    //System.Console.WriteLine("Horizontal: '{0}'/{1}, Vertical: '{2}'", horizontalPosition.HorizontalPositionTypes.Name,
                    //    horizontalPosition.HorizontalPositionTypeId, horizontalPosition.VerticalPositionTypes.Name);
                }
                output.AppendLine();
            }

            return output.ToString();
        }

        public static double CalculateBrigadeFormationWinPercent(List<FamiliarWinPercent> familiarWinPercents, BrigadeFormation formation,
            bool includeReserve = true)
        {
            double formationWinPercent = 0;
            double totalWinPercent = 0;

            int numFormationPositions = formation.Positions.Count;
            int currentFamiliarIndex = 0;
            foreach (var position in formation.Positions)
            {
                totalWinPercent += familiarWinPercents[currentFamiliarIndex].WinPercents[position.VerticalPositionTypes];
                currentFamiliarIndex++;
            }
            if (includeReserve)
            {
                foreach (var position in formation.Positions)
                {
                    totalWinPercent += familiarWinPercents[currentFamiliarIndex].WinPercents[position.VerticalPositionTypes];
                    currentFamiliarIndex++;
                }
            }
            System.Console.WriteLine("Total win percent{0}: {1}", (includeReserve ? " (with reserve)" : string.Empty),
                totalWinPercent);

            formationWinPercent = totalWinPercent / (includeReserve ? numFormationPositions * 2 : numFormationPositions);
            return formationWinPercent;
        }

        public static IEnumerable<BrigadeFormationWithFamiliars> GetBrigadeFormationPermutations(PhlebotomistRepository phlebotomistRepository,
            List<FamiliarWinPercent> familiarWinPercents, BrigadeFormationWithFamiliars currentFormation,
            bool includeReserve)
        {
            // Either no more familiars left to place or we've placed everyone already.
            if (familiarWinPercents.Count == 0 ||
                currentFormation.NumFamiliarsInPosition == (currentFormation.Formation.NumPositions *
                (currentFormation.IncludeReserve ? 2 : 1)))
            {
                yield return currentFormation;
            }

            for (int i = 0; i < familiarWinPercents.Count; i++)
            {
                bool placingReserveFamiliar = currentFormation.NumFamiliarsInPosition >= currentFormation.Formation.NumPositions &&
                    currentFormation.IncludeReserve;

                // The following can not be translated into a LINQ to Entities expression, so we have to do it the hard way. :(
                //var positionsWithOpenSlots = phlebotomistRepository.Context.BrigadeFormationVerticalPositionTypes.Where(x =>
                //    currentFormation.HasOpenSlotsInPosition(x));  // same as the following line
                //    //currentFormation.FamiliarsInPositions[x].Count < currentFormation.MaxFamiliarsInPositions[x] / (placingReserveFamiliar ? 1 : 2));

                var positionsWithOpenSlots = new List<BrigadeFormationVerticalPositionType>();
                foreach (var verticalPosition in phlebotomistRepository.Context.BrigadeFormationVerticalPositionTypes)
                {
                    if (currentFormation.HasOpenSlotsInPosition(verticalPosition))
                    {
                        positionsWithOpenSlots.Add(verticalPosition);
                    }
                }

                var nextFamiliar = familiarWinPercents[i];
                foreach (var verticalPosition in positionsWithOpenSlots)
                {
                    BrigadeFormationWithFamiliars nextFormation = new BrigadeFormationWithFamiliars(currentFormation);
                    var remainingFamiliars = new List<FamiliarWinPercent>(familiarWinPercents);
                    remainingFamiliars.RemoveAt(i);

                    nextFormation.FamiliarsInPositions[verticalPosition].Add(nextFamiliar);
                    foreach (var permutation in GetBrigadeFormationPermutations(phlebotomistRepository, remainingFamiliars,
                        nextFormation, includeReserve))
                    {
                        yield return permutation;
                    }
                }
            }
        }

        static IEnumerable<BrigadeFormationWithFamiliars> GetBrigadeFormationPermutations(PhlebotomistRepository phlebotomistRepository,
            List<FamiliarWinPercent> familiarWinPercents, BrigadeFormation formation, bool includeReserve)
        {
            var currentBrigadeInstace = new BrigadeFormationWithFamiliars(phlebotomistRepository);
            currentBrigadeInstace.Formation = formation;
            foreach (var brigadeFormation in GetBrigadeFormationPermutations(phlebotomistRepository, familiarWinPercents,
                currentBrigadeInstace, includeReserve))
            {
                yield return brigadeFormation;
            }
        }

        static void Main(string[] args)
        {
            PhlebotomistModelContainer brigadeContext = new PhlebotomistModelContainer();
            PhlebotomistRepository phlebotomistRepository = new Phlebotomist.Repositories.PhlebotomistRepository(brigadeContext);

            var familiarWinPercents = ReadInputFile(phlebotomistRepository, _inputFileName);

            var fiveFamiliarBrigadeFormations = phlebotomistRepository.Context.BrigadeFormations.Where(x => x.NumPositions == 5);
            /*
            System.Console.WriteLine("Found {0} brigade formation with five familiars.", fiveFamiliarBrigadeFormations.Count());
            foreach (var brigadeFormation in fiveFamiliarBrigadeFormations)
            {
                System.Console.WriteLine(GetBrigadeFormationString(phlebotomistRepository, brigadeFormation));
                double formationWinPercent = CalculateBrigadeFormationWinPercent(familiarWinPercents,
                    brigadeFormation);
                System.Console.WriteLine("Win percent for formation '{0}': {1}", brigadeFormation.Name,
                    formationWinPercent);
            }
             * */

            int num = 0;
            var brigadeFormationPermuations = GetBrigadeFormationPermutations(phlebotomistRepository, familiarWinPercents,
                fiveFamiliarBrigadeFormations.FirstOrDefault(), true);
            foreach (var permutation in brigadeFormationPermuations)
            {
                num++;
                System.Console.Write("{0}: ", num);
                System.Console.WriteLine(permutation.ToString());
            }

            System.Console.WriteLine("Found {0} familiar types.", phlebotomistRepository.Context.FamiliarTypes.Count());
            foreach (var familiarType in phlebotomistRepository.Context.FamiliarTypes)
            {
                System.Console.WriteLine("Brigade Formation: {0}", familiarType.Name);
            }

            System.Console.WriteLine("Done.");
            System.Console.ReadLine();
        }
    }
}
