using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyTicket.App_UI.Forms.Main_Menu.Account_Settings
{
    public partial class Change_Password : Form
    {
        public void SetLoggedInUsername(string user)
        {
            labelUsername.Text = user;
        }
        DB db = new DB();
        public Change_Password()
        {
            InitializeComponent();
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string user = labelUsername.Text;
            string new_password = txtNewPassword.Text;
            if (!CheckBlankFields() && CheckMatchingPasswords() && (db.UpdatePassword(user, new_password)))
            {
                MessageBox.Show("Your password has been changed");
            }
            else
            {
                MessageBox.Show("Your password could not be changed");
            }
        }

        private bool CheckBlankFields()
        {
            if((txtUsername.Text == "") || (txtOldPassword.Text == "") || (txtNewPassword.Text == "") || (txtConfirmNewPassword.Text == ""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckMatchingPasswords()
        {
            string password = txtNewPassword.Text;
            string confirm_password = txtConfirmNewPassword.Text;

            if(password == confirm_password)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
