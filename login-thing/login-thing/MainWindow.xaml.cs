using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents; //I don't need half of these. Oh well.
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
using System.Xml.Serialization;

namespace login_thing
{
    /// <summary>    
    ///This class is for my implementation of inotifypropertychanged, this allows the inputs for username, password etc to be updated in real time in the code
    ///the code in this class would usually go in the main class for the program but as i have two classes, login and signup, i thought it would be better to
    ///create a base class that both of those inherit from to avoid writing the same code twice.     
    /// </summary>

    //[INTERFACE STUFF]-----------------------------------------------------------------------------------------------
    public class BaseNotify : INotifyPropertyChanged //The colon here means that this class inherits from the interface INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; //This is the implementation of that interface with the event PropertyChanged

        public void Notify(string property) //This method essentially checks if the property has changed since the last check and then triggers the event if it has
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));                
            }
        }
    }

    public class Command : ICommand //This class is needed so that the functions I write here can interact with the gui, you can pretty much ignore it
    {
        public event EventHandler CanExecuteChanged;

        private Action command;

        public Command(Action command)
        {
            this.command = command;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (command != null) command();
        }
    }
    //[/INTERFACE STUFF]-----------------------------------------------------------------------------------------------

    //[SIGNUP/LOGIN STUFF]--------------------------------------------------------------------------------------------------
    public class SignUp : BaseNotify //As you can see here, this class inherits from the BaseNotify class I wrote earlier, allowing it to use everything in that class
    {        
        //These statements are properties for the SignUp class, these will retrieve the appropriate bits of data from the gui
        //A property in C# is essentially a function that returns a value, it has a getter and a setter, in WPF, GUI elements can only interact
        //with properties, not variables, hence why we have separate variables for storing the data once it has been retrieved.
        public string forename { get { return _forename; } set { _forename = value; Notify("forename"); } }
        public string surname { get { return _surname; } set { _surname = value; Notify("surname"); } }
        public string username { get { return _username; } set { _username = value; Notify("username"); } }
        public string email { get { return _email; } set { _email = value; Notify("email"); } }
        public string password { get { return _password; } set { _password = value; Notify("password"); } }

        //These variables are what the data will be stored in when it is retrieved
        private string _forename;
        private string _surname;
        private string _username;
        private string _email;
        private string _password;
        
        //What I'm doing here is the same as what I did earlier with the properties forename, surname etc, only here I'm doing it with the function
        //that will execute when a button is pressed.
        public Command SignUpCommand {  get { return _signUpCommand; } }
        private Command _signUpCommand;

        public SignUp() //This is the class initializer, it creates an instance of the command class I made earlier and puts in the signUp method
        {               //as a parameter.
            _signUpCommand = new Command(signUp);
        }

        public void signUp() //This is the signUp function which executes when the signup button is pressed.
        {                    //Here, public is the protection level, public meaning it can be accessed from outside of this class
                             //The Void keyword is the return type of the function, this function doesn't return anything, so void is used.
            UserList curFile = new UserList(); //This creates a new instance of the UserList class and assigns it to the variable "curFile"
            //This block here creates a new instance of the user class, but as you can see, it sets the values of the variables in that class
            //to the data that was retrieved from the GUI.
            User newUser = new User
            {
                username = _username,
                password = _password,
                forename = _forename,
                surname = _surname,
                email = _email
            };
            
            //This is one big if statement that checks if any of the data retrieved from the text boxes is an empty string or null.
            //Here, the || operator is equivalent to "or" in python
            if (string.IsNullOrEmpty(newUser.username) == true || 
                string.IsNullOrEmpty(newUser.password) == true || 
                string.IsNullOrEmpty(newUser.forename) == true ||
                string.IsNullOrEmpty(newUser.surname) == true ||
                string.IsNullOrEmpty(newUser.email) == true)
            {
                MessageBox.Show("You must fill all fields.");
                return;
            }

            //This is a length check for the username.
            if (newUser.username.Length > 10)
            {
                MessageBox.Show("The username must be no longer than 10 characters.");
                return; //This return statement at the end of all the ifs basically exits out of the function
            }
            //All of these next validation checks are done using regex, I used regex because it lets me do type checks, format checks and length
            //checks all in one go, making the code much neater.
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

            //Here I've retrieved the users from the file
            List<User> tempUsers = curFile.LoadDatabase();
            foreach (User x in tempUsers) //This for loop combined with an if statement checks if the user being added has the same username as
            {                             //an existing user.
                if (x.username == _username)
                {
                    MessageBox.Show("A user with that username already exists.");
                    return;
                }
            }

            string encryptedPass = PasswordHash.CreateHash(newUser.password); //This encrypts the password and replaces the original with the new
            newUser.password = encryptedPass; //encrypted one, the code for this is in Encryption.cs

            //If the data passed all the checks then it is added to the list of users
            curFile.users.Add(newUser);

            //Then is is saved
            curFile.SaveDatabase();
            //Then the text fields are cleared
            clear();
        }

        public void clear() //Because the data binding is two way, if I set the username, forename etc properties to something, the text in the
        {                   //textboxes will also change.
            username = "";
            forename = "";
            surname = "";
            email = "";
            password = "";
        }
    }

    public class Login : BaseNotify //This class is similar to the SignUp class but for logging in
    {
        private Command _loginCommand;
        public Command loginCommand { get { return _loginCommand; } }//Here we have the command stuff I mentioned earlier

        public Login()//Init
        {
            _loginCommand = new Command(login);
        }

        public string username { get { return _username; } set { _username = value; Notify("username"); } }//Properties
        public string password { get { return _password; } set { _password = value; Notify("password"); } }

        private string _username;//Storing vars
        private string _password;
        
        public void login()//Login function
        {
            UserList curFile = new UserList();
            List<User> curUsers = curFile.LoadDatabase();

            foreach (User x in curUsers)
            {
                if (x.username == _username && PasswordHash.ValidatePassword(_password, x.password) == true)//Checks if the username and password match anything in the file
                {
                    MessageBox.Show("Welcome "+x.forename+", You have logged in successfully.");
                    return;
                }
            }

            MessageBox.Show("Username/password is incorrect or the user doesn't exist.");
        }
    }
    //[/SIGNUP/LOGIN STUFF]-----------------------------------------------------------------------------------------------

    //[FILE STUFF]----------------------------------------------------------------------------------------------------
    public class UserList //Class for all the saving and loading to file. I was going to use SQL but it's a massive hassle in C#, needs servers and things
    {
        public string filePath;

        public List<User> users;

        public UserList()
        {
            filePath = AppDomain.CurrentDomain.BaseDirectory + "users.xml"; //Gets the current file path of the program's exe

            users = new List<User>();
        }

        public List<User> LoadDatabase()//Loads the data from file, note how the return type here is "List<User>" meaning all the possible outcomes
        {                               //of this method must return a list of user objects
            if (!File.Exists(filePath))
            {
                users = new List<User>();
                return users;
            }
            else
            {
                XmlSerializer serialiser = new XmlSerializer(typeof(List<User>)); //New instance of the .NET XmlSerializer
                using (StreamReader sr = new StreamReader(filePath)) //This using thing means that the StreamReader is disposable, meaning that
                {                                                    //once it's finished doing what it's doing, the object is disposed of
                                                                     //meaning that we won't have multiple things trying to access the same file

                    users = (List<User>)serialiser.Deserialize(sr); //This right here is straight up magic, it takes the XML data and turns it into
                }                                                   //a list of user objects

                return users; //Return the list
            }
        }

        public void SaveDatabase() //This does the opposite of the load method, takes a list and converts it to XML data
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(List<User>));
            //serialiser.Serialize(new StreamWriter(filePath), users);
            using (StreamWriter sr = new StreamWriter(filePath))
            {
                serialiser.Serialize(sr, users);
            }
        }
    }
    
    public class User //My user class, which is used to store data for each user.
    {        
        public string username;     
        public string password;        
        public string forename;        
        public string surname;        
        public string email;
    }
    //[/FILE STUFF]-------------------------------------------------------------------------------------------------

    public partial class MainWindow : Window //This is the class that deals with GUI things
    {
        public Login login { get; set; }//This sets the login class as a property of the mainwindow class
        public SignUp signUp { get; set; }//This does the same for the SignUp class
        public MainWindow()
        {
            signUp = new SignUp(); //Initializes the SignUp and Login classes allowing the GUI to use them
            login = new Login();


            DataContext = this; //The fact that the other two classes are properties of this class allow me to set this class as the data context of itself
                                //This means that I can specify whether a gui element is for Signing Up or Logging in by putting the data context as
                                //login.whatever or signup.whatever            
            
            InitializeComponent(); //This was generated by Visual Studio, I don't know what it does, it's probably evil.
        }        
    }
}
