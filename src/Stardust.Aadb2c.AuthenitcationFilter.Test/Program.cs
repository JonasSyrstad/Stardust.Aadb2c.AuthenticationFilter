using System;
using System.Collections.Specialized;
using Microsoft.IdentityModel.Logging;
using Stardust.Aadb2c.AuthenticationFilter.Core;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenitcationFilter.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ConfigurationManagerHelper.SetManager(new DummyManager());
                B2CGlobalConfiguration.AadPolicy = "B2C_1A_SignInWithADFSIdp";
                B2CGlobalConfiguration.AadTenant = "dnvglb2ctest.onmicrosoft.com";
                B2CGlobalConfiguration.Audience = "aa307cec-db8a-4224-8d5f-e864f16b1ad0";
                IdentityModelEventSource.ShowPII = true;
                //B2CGlobalConfiguration.AudienceV1 = "https://dnvglb2ctest.onmicrosoft.com/af7f55bc-5f35-44a7-a18a-c62ef80bdd5d";
                B2CGlobalConfiguration.AudienceV1 = "https://dnvglb2ctest.onmicrosoft.com/efb3e529-2f80-458b-aedf-7f4c8c794b45";
                B2CGlobalConfiguration.ValidIssuer = "https://login.microsoftonline.com/ed815121-cdfa-4097-b524-e2b23cd36eb6/v2.0";
                B2CGlobalConfiguration.ValidIssuerV1 = "https://sts.windows.net/ed815121-cdfa-4097-b524-e2b23cd36eb6/";
                Console.WriteLine("Validating bearer token");
                TokenValidator.SetLogger(new ConsoleLogger());
                TokenValidator.Validate(string.Join("", args));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed");
            }
            Console.ReadKey();
        }
    }

    internal class DummyManager : IConfigurationReader
    {
        public NameValueCollection AppSettings => new NameValueCollection();
    }

    internal class ConsoleLogger : ILogging
    {
        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            var r = string.IsNullOrWhiteSpace(additionalDebugInformation)
                  ? ""
                  : string.Format("({0})", additionalDebugInformation);
            Console.WriteLine($"{exceptionToLog.Message} {r}");
            Console.WriteLine(exceptionToLog.StackTrace);
            Console.ForegroundColor = oldColor;
            if (exceptionToLog.InnerException != null)
                Exception(exceptionToLog.InnerException, "inner");
        }

        public void HeartBeat()
        {

        }

        public void DebugMessage(string message, LogType entryType = LogType.Information, string additionalDebugInformation = null)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            var r = string.IsNullOrWhiteSpace(additionalDebugInformation)
                ? ""
                : string.Format("({0}) ", additionalDebugInformation);
            Console.WriteLine($"{r}{message}");
            Console.ForegroundColor = oldColor;
        }

        public void SetCommonProperties(string logName)
        {

        }
    }
}
