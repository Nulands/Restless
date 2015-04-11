using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nulands.Nubilu.UI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        private object _value;
        private TypeCode _typeCode;

        public InputDialog()
        {
            InitializeComponent();
        }

        public InputDialog(WindowState state = System.Windows.WindowState.Normal)
        {
            InitializeComponent();
            this.WindowState = state;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            bool result = true;
            try
            {
                _value = System.Convert.ChangeType(tbInput.Text, _typeCode);
            }
            catch (Exception exc)
            {
                result = false;
            }
            finally
            {
                DialogResult = result;
            }
        }

        public void Show(TypeCode typeCode, string header)
        {
            _typeCode = typeCode;
            lHeader.Content = header;
            this.Show();
        }

        public bool? ShowDialog(TypeCode typeCode, string header)
        {
            _typeCode = typeCode;
            lHeader.Content = header;
            return this.ShowDialog();
        }

        public TypeCode TypeCode
        {
            get { return _typeCode; }
        }

        public T Value<T>()
        {
            return (T)_value;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbInput.Focus();
        }
    }
}
