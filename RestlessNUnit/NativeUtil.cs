using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nulands.Nubilu.UI;


namespace Nulands.Nubilu.UI
{
    public class NativeUtil
    {
        public static void StartAuthorization(string url)
        {
            Process process = Process.Start(url);
        }

        public static string PromptForAuthCode(System.Windows.Window win = null)
        {
            string authCode = "";
            InputDialog dialog = new InputDialog(System.Windows.WindowState.Normal);
                dialog.Owner = win;

            dialog.Height = dialog.Owner.Height;
            dialog.Width = dialog.Owner.Width;

            if ((bool)dialog.ShowDialog(TypeCode.String, "Please enter the authorization code: "))
            {
                authCode = dialog.Value<string>();
            }

            return authCode;
        }
    }
}
