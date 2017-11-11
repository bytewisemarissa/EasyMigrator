using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMigrator.Utility
{
    public static class ConsoleUtility
    {
        public static string ComposeFullName()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.Append(
                @"
  ______                       __  __ _                 _             
 |  ____|                     |  \/  (_)               | |            
 | |__   __ _ ___ _   _       | \  / |_  __ _ _ __ __ _| |_ ___  _ __ 
 |  __| / _` / __| | | |      | |\/| | |/ _` | '__/ _` | __/ _ \| '__|
 | |___| (_| \__ \ |_| |      | |  | | | (_| | | | (_| | || (_) | |   
 |______\__,_|___/\__, |      |_|  |_|_|\__, |_|  \__,_|\__\___/|_|   
                   __/ |                 __/ |                        
                  |___/                 |___/                         
");
            builder.AppendLine();
            builder.AppendLine("A .NET Core datbase migration utility targeted for MySQL.");
            builder.AppendLine();

            return builder.ToString();
        }

        public static void PerformWarning()
        {
            Console.WriteLine();
            Console.WriteLine("!!      WARNING     !!");
            Console.WriteLine();
            Console.WriteLine(
                "You are about to make changes to a database. Please ensure that you have taken a back up.");

            while (true)
            {
                Console.Write("Type 'okay' to continue: ");
                string userInput = Console.ReadLine();

                if (userInput.ToLower() == "okay")
                {
                    break;
                }
            }
        }
    }
}
