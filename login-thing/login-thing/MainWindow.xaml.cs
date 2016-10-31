using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Data.Linq;
using System.Data.Sql;
using System.Data.Linq.Mapping;
using System.Text.RegularExpressions;

namespace login_thing
{
    /// <summary>    
    ///This class is for my implementation of inotifypropertychanged, this allows the inputs for username, password etc to be updated in real time in the code
    ///the code in this class would usually go in the main class for the program but as i have two classes, login and signup, i thought it would be better to
    ///create a base class that both of those inherit from to avoid writing the same code twice.     
    /// </summary>

    public class BaseNotify : INotifyPropertyChanged //The colon here means that this class inherits from the interface INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; //This is the implementation of that interface with the event PropertyChanged

        public void Notify(string property) //This method essentially checks if the property has changed since the last check and then triggers the event if it has
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class SignUp : BaseNotify //As you can see here, this class inherits from the BaseNotify class I wrote earlier, allowing it to use everything in that class
    {        
        //These statements are properties for the SignUp class, these will retrieve the appropriate bits of data from the gui
        public string forename { get { return forename; } set { _forename = value; Notify("forename"); } }
        public string surname { get { return surname; } set { _surname = value; Notify("surname"); } }
        public string username { get { return username; } set { _username = value; Notify("username"); } }
        public string email { get { return email; } set { _email = value; Notify("email"); } }
        public string password { get { return password; } set { _password = value; Notify("password"); } }

        //These variables are what the data will be stored in when it is retrieved
        private string _forename;
        private string _surname;
        private string _username;
        private string _email;
        private string _password;

        public void signUp()
        {
            UserDB db = new UserDB(AppDomain.CurrentDomain.BaseDirectory + @"\users.mdf");
            User newUser = new User
            {
                username = _username,
                password = _password,
                forename = _forename,
                surname = _surname,
                email = _email
            };
            
            //TODO: Presence check

            if (newUser.username.Length > 10)
            {
                MessageBox.Show("The username must be no longer than 10 characters.");
                return;
            }
            if (!Regex.IsMatch(newUser.password, @"^(?=.*[A-Z])(?=.*\d)[a-zA-Z0-9]{6,12}$"))
            {
                MessageBox.Show("The password must be between 6 and 12 characters and contain at least one number and one capital letter.");
                return;
            }
            if (!Regex.IsMatch(newUser.forename, @"^(?!.*\s)(?!.*\d)[A-Za-z]{1,12}$"))
            {
                MessageBox.Show("Your forename must not contain any special characters, spaces or numbers");
                return;
            }

            if (!Regex.IsMatch(newUser.surname, @"^(?!.*\s)(?!.*\d)[A-Za-z\-]{1,12}$"))
            {
                MessageBox.Show("Your surname must not contain any numbers, spaces or special characters except '-'");
                return;
            }
            if (!Regex.IsMatch(newUser.email, @"^[a-z0-9](\.?[a-z0-9_-]){0,}@[a-z0-9-]+\.([a-z]{1,6}\.)?[a-z]{2,6}$"))
            {
                MessageBox.Show("Please enter a valid email");
                return;
            }
        }
    }

    public class Login : BaseNotify
    {

        public string username { get { return username; } set { _username = value; Notify("username"); } }
        public string password { get { return password; } set { _password = value; Notify("password"); } }

        private string _username;
        private string _password;
    }

    //<DATABASE STUFF------------------------------------------------------------------------------------------------>
    public class UserDB : DataContext
    {
        public Table<User> Users;
        public UserDB(string connection) : base(connection) { }

        public void addUser(User user)
        {
            return;
        }

        public void checkUser(User user)
        {
            return;
        }
    }

    [Table(Name = "UserTable")]
    public class User
    {
        [Column(IsPrimaryKey = true)]
        public string username;
        [Column]
        public string password;
        [Column]
        public string forename;
        [Column]
        public string surname;
        [Column]
        public string email;
    }

    public class Repository
    {
        public void addUser (User user, UserDB db)
        {
            return;
        }

        public void checkUser (User user)
        {
            return;
        }
    }
    //</DATABASE STUFF------------------------------------------------------------------------------------------------>

    public partial class MainWindow : Window
    {
        public Login login { get; set; }//This sets the login class as a property of the mainwindow class
        public SignUp signUp { get; set; }//This does the same for the SignUp class
        public MainWindow()
        {
            DataContext = this; //The fact that the other two classes are properties of this class allow me to set this class as the data context of itself
                                //This means that I can specify whether a gui element is for Signing Up or Logging in by putting the data context as
                                //login.whatever or signup.whatever            
            string[] dir = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
            if (dir.Contains("users.mdf") == false)
            {
                UserDB db = new UserDB(AppDomain.CurrentDomain.BaseDirectory+@"\users.mdf");
                db.CreateDatabase();
            }
            InitializeComponent();
        }
    }
}
