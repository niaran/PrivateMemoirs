
using System.ComponentModel;
using System.Configuration.Install;

namespace PrivateMemoirs
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}