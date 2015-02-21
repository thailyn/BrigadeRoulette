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
    class Program
    {
        private static string _inputFileName = "input.csv";

        static void ReadInputFile(string fileName)
        {
            using (var streamReader = new StreamReader(_inputFileName))
            {
                while (!streamReader.EndOfStream)
                {
                    var columns = streamReader.ReadLine().Split(',');
                    foreach (var column in columns)
                    {
                        System.Console.Write("'{0}'\t", column);
                    }
                    System.Console.WriteLine();
                }
            }
        }

        static void Main(string[] args)
        {
            ReadInputFile(_inputFileName);

            System.Console.WriteLine("Test.");
            System.Console.ReadLine();

            PhlebotomistModelContainer brigadeContext = new PhlebotomistModelContainer();
            PhlebotomistRepository phlebotomistRepository = new Phlebotomist.Repositories.PhlebotomistRepository(brigadeContext);

            var brigadeFormations = phlebotomistRepository.Context.BrigadeFormations.Where(x => x.NumPositions == 5);
            System.Console.WriteLine("Found {0} brigade formation.", brigadeFormations.Count());
            foreach (var brigadeFormation in brigadeFormations)
            {
                System.Console.WriteLine("Brigade Formation: {0}", brigadeFormation.Name);
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
