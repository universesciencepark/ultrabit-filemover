using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ultrabit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Count() > 0)
            {
                    this.Properties["Filename"] = e.Args[0];
            }

            if(AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null) 
            {
                try
                {
                    this.Properties["Filename"] = new Uri(AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0]).LocalPath;
                } catch (Exception ex)
                {

                }
            }

            base.OnStartup(e);
        }
    }
}
