using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Translator
{
    public class BingTranslator
    {
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            { 
                Console.WriteLine("Usage: translate \"text\" <source> <dest>");
                return;
            }
            
            var inputString = args[0];

            Console.OutputEncoding = Encoding.UTF8;

            TranslatorContainer tc = TranslatorContainer.Initialize();

            var sourceLanguage = args.Length >= 2 ? args[1] : "en";
            var targetLanguage = args.Length >= 3 ? args[2] : "pt";

            var translationResult = tc.Translate(inputString, targetLanguage, sourceLanguage);

            foreach (var translation in translationResult)
            {
                Console.WriteLine(translation.Text);
            }
        }
    }
}