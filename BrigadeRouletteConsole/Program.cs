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
                    var columns = streamReader.ReadLine().Split(',');
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

        static void PrintBrigadeFormations(PhlebotomistRepository phlebotomistRepository,
            IQueryable<BrigadeFormation> formations)
        {
            var rows = phlebotomistRepository.Context.BrigadeFormationVerticalPositionTypes.OrderByDescending(x => x.DamageDealtModifier);
            foreach (var brigadeFormation in formations)
            {
                System.Console.WriteLine("Brigade Formation: {0}", brigadeFormation.Name);
                foreach (var row in rows)
                {
                    // using the assumption that HorizontalPositionTypeId increases from left to right
                    foreach (var horizontalPosition in brigadeFormation.Positions.OrderBy(x => x.HorizontalPositionTypeId))
                    {
                        if (horizontalPosition.VerticalPositionTypes.Id == row.Id)
                        {
                            System.Console.Write("*");
                        }
                        else
                        {
                            System.Console.Write(" ");
                        }
                        //System.Console.WriteLine("Horizontal: '{0}'/{1}, Vertical: '{2}'", horizontalPosition.HorizontalPositionTypes.Name,
                        //    horizontalPosition.HorizontalPositionTypeId, horizontalPosition.VerticalPositionTypes.Name);
                    }
                    System.Console.WriteLine();
                }
            }
        }

        static void Main(string[] args)
        {
            ReadInputFile(_inputFileName);

            PhlebotomistModelContainer brigadeContext = new PhlebotomistModelContainer();
            PhlebotomistRepository phlebotomistRepository = new Phlebotomist.Repositories.PhlebotomistRepository(brigadeContext);

            var fiveFamiliarBrigadeFormations = phlebotomistRepository.Context.BrigadeFormations.Where(x => x.NumPositions == 5);
            System.Console.WriteLine("Found {0} brigade formation with five familiars.", fiveFamiliarBrigadeFormations.Count());
            PrintBrigadeFormations(phlebotomistRepository, fiveFamiliarBrigadeFormations);


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
