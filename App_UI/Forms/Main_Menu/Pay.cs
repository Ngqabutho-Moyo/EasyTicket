using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace EasyTicket.Forms.Main_Menu
{
    public partial class Pay : Form
    {
        DB db = new DB();
        DataTable dt = new DataTable();        
        Main_Menu mm = new Main_Menu();
        //OTP otp = new OTP();
        VerifyPassword verify = new VerifyPassword();

        public void LoggedInUser(string username)
        {
            labelUsername.Text = username;
        }

        public Pay()
        {
            InitializeComponent();
        }

        //Only a-z and A-Z are allowed in the 'from' and 'to' textboxes
        private bool ValidateFromTo(string from, string to)
        {
            string regex = "[A-Za-z]";
            Regex r = new Regex(regex);
            if (r.IsMatch(from) && r.IsMatch(to))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        //Check for blank fields
        private bool CheckBlankFields()
        {
            if(txtCurrentBalance.Text == "" || txtFrom.Text == "" || txtTo.Text == "" || txtPrice.Text == "") 
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Does the commuter have sufficient funds?
        private bool CheckBalanceAndPrice(double balance, double price)
        {
            if (balance < price)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //No free trips
        private bool ZeroPayment(double payment)
        {
            if (payment == 0.0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Deduct trip price from commuter's balance
        private double CalculateNewBalance(double current_balance, double price)
        {
            return current_balance - price;
        }

        //Event: pay for a trip; deduct appropriate amount from commuter's account        
        private void btnPay_Click_1(object sender, EventArgs e)
        {   
            string username = labelUsername.Text;
            string from = txtFrom.Text;
            string to = txtTo.Text;            
            double price = Convert.ToDouble(txtPrice.Text);
            double current_balance = Convert.ToDouble(txtCurrentBalance.Text);
            double new_balance = CalculateNewBalance(current_balance, price);

            //No field should be left blank
            if(!CheckBlankFields())
            {
                //Are the 'from' and 'to' fields a-z and/or A-Z?
                if(ValidateFromTo(from, to))
                {
                    //If there are sufficient funds, process the ticket
                    if (!CheckBalanceAndPrice(current_balance, price))
                    {
                        //Price cannot be 0.00
                        if (!ZeroPayment(price))
                        {
                            //Verify the user's password before paying                    
                            verify.LoggedInUser(username);
                            if (verify.ShowDialog() == DialogResult.OK)
                            {
                                if (verify.is_verified && db.UpdateCommuterBalance(username, new_balance) && db.InsertIntoTrip(username, from, to, price))
                                {
                                    MessageBox.Show($"{DateTime.Now}\n\nYou paid: ${price} \nFrom: {from} \nTo: {to}\nYour new balance is ${new_balance}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadDataGridView();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to update account", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Transaction failed", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Price cannot be 0.00", "Invalid price", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    //Else raise an error for insufficient funds
                    else
                    {
                        MessageBox.Show("You have insufficient funds for this journey", "Insufficient funds", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Your route details are invalid", "Invalid Route", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Fill in all fields", "Blank Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //Call LoadDataGridView() when the form loads
        private void Pay_Load(object sender, EventArgs e)
        {
            LoadDataGridView();
        }

       

        //Automatically populate the commuter's balance inside the 'Current Balance' textbox
        private void dgvPay_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = this.dgvPay.Rows[e.RowIndex];
            txtCurrentBalance.Text = row.Cells["balance"].Value.ToString();
        }

        //Populate the datagrid view with the commuter's account details
        private void LoadDataGridView()
        {
            string username = labelUsername.Text;
            string select = "SELECT first_name, last_name, balance FROM public.commuter WHERE username=@username";
            using (NpgsqlConnection connection = db.GetConnection())
            {
                NpgsqlCommand command = new NpgsqlCommand(select, connection);
                command.Parameters.AddWithValue(@"username", username);
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                da.Fill(dt);
                dgvPay.DataSource = dt;
            }
        }

        //Close the application when the user closes the form
        private void Pay_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        //Navigate back to the main menu
        private void llMainMenu_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            mm.LoggedInUsername(labelUsername.Text);
            this.Hide();
            mm.Show();
        }
    }    
}
