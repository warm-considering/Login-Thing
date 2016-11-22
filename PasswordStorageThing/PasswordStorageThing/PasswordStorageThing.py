import pickle
import os
import os.path
import hashlib
import string
import random

#This is a program that uses pickle and dictionaries to store passwords.
#The dict key is the service that the password is for, i.e. PayPal, Steam, whatever
#The other bit is the password.
#When saving the passwords to a file, it pickles the dictionary, encrypts it against a user defined master password for that file using XOR
#Then saves it.
#The master password gets hashed before it is used to XOR the data.
#When opening a file, the user is asked for that file's master password, the program then hashes that, and uses it to XOR the data in the file
#If the result is readable by pickle, the password was correct, if it's just nonsense, then the password is not correct.

#All this works because XORing something is the opposite of XORing something, so it's reversible as long as the hashed password is the same.
#It could be more secure if it used proper encryption, but I couldn't get PyCrypto to install via pip, so I went with this instead.

passwords = {} #Creates the dictionary that will store the passwords
chars = string.ascii_letters + string.digits #Creates a variable to store all alphanumeric characters, needed later


def openFile(): #Method for opening files already there
    global passwords #Allows this method to access the global passwords dict
    print("Password files in this directory: ")
    for i in os.listdir(): #This lists any files in the directory that have the file extension ".pass"
        if i.endswith(".pass") == True:
            print(i)
    filename = input("Enter the name of the file you want to open: ") 
    if os.path.exists(filename) == False: #If the user tries to open a file that doesn't exist, tell them
        print("This file does not exist")
        return
    file = open(filename, 'r+b') #Opens the file
    encryptedPasses = file.read() #Reads it
    password = input("Enter the master password for this file: ")
    hashedMaster = hashlib.sha1(password.encode(encoding="UTF-8")).digest() #Hashes the master password using sha1
    decryptedPasses = logicalXOR(encryptedPasses, hashedMaster) #This XORs the encrypted passwords file using the the hashed password as the key
    try: #If the password to the file was correct, this should work as the data will now be in its original pickled format
        passwords = pickle.loads(decryptedPasses)
    except UnpicklingError: #If the password is wrong, the XOR will return basically a load of nonsense which pickle can't read, thus throwing this exception
        print("Incorrect password.")
        return
    print("Here is a list of your passwords and services: ")
    for i in passwords: #Assuming everything worked properly, this prints a list of the passwords stored in that file
        print(i+": "+passwords[i])

    while True: #This is like a little options menu for what to do with the opened file
        print("1. Add a random password \n2. Add a manual password \n3. Delete a password \n4. Finish")
        action = int(input())
        if action == 4:
            break
        fileChoices[action - 1]() #List of functions which can be called using list index, quite spicy I thought

    file.seek(0) #Once the user has done all the things with the file that they want, this goes to position 0 in the file
    file.truncate() #This deletes everything already in there

    pickledPasses = pickle.dumps(passwords)
    file.write(logicalXOR(pickledPasses, hashedMaster)) #This encrypts the data against the hashed master password and then writes it to the file.
    file.close() #Closes file.


def createFile(): #This method is for making new password files
    global passwords #Allows access to the global passwords dict, almost certainly a bad idea, but it works
    filename = input("Enter the name of the file you want to make: ")
    filepass = input("Enter the desired master password for this file: ")
    if os.path.exists(filename): #Checks if the file being made already exists
        print("This file already exists, cancelling operation.")
        return    
    while True: #Same little menu thing as earlier
        print("1. Generate a random password \n2. Manually add a password. \n3. Delete a password \n4. Finish")
        action = int(input())
        if action == 4: #Breaks out of the while if the user chooses "Finish"
            break
        fileChoices[action - 1]() #The same nice function list thing, wayyy better than loads of if/elifs I reckon

    pickledPasses = pickle.dumps(passwords) #Pickles the dictionary
    hashedMaster = hashlib.sha1(filepass.encode(encoding="UTF-8")).digest() #Hashes the master password for the file
    file = open(filename+".pass", 'wb') #Creates file
    file.write(logicalXOR(pickledPasses, hashedMaster)) #Encrypts data and writes it to the file
    file.close() #Closes file, commiting the changes


def randPass(): #This is used to produce randomized passwords
    global passwords #Allows access to the global passwords dict, this lets me run this from another function and still be able to access the changes made here, admittedly, there are other, better ways of doing this, but this was what popped into my head first
    service = input("This password is for: ")
    passLen = int(input("Enter the length you want for the password: "))
    random.seed = os.urandom(1024) #This creates a new random seed for the random functions
    randomPass = ''.join(random.choice(chars) for i in range(passLen)) #This chooses random chars out of that char array thing at the start, using the new seed for extra spicy randomness.
    print("The password is: "+randomPass)
    passwords[service] = randomPass #Adds the password and associated service (IE the thing the password is for) to the dict

def delPass(): #For deleting passwords, pretty self explanatory
    global passwords
    service = input("Enter the name of the service you want to delete: ")
    del passwords[service]
    print(service+" deleted.")


def manPass(): #This is for adding user defined passwords, so, not randomly generated, to the dict, pretty simple stuff
    global passwords
    service = input("This password is for: ")
    password = input("Enter the password: ")
    passwords[service] = password

def logicalXOR(passes, masterpass): #This is the XORing thing I made, it takes the pickled dict, which is a byte array, and XORs each byte against each byte of the master password for that file
    bytes1 = bytearray(passes)
    bytes2 = bytearray(masterpass)
    for i in range(0, len(bytes1)-1): #This is where the XORing happens
        bytes1[i] = bytes1[i] ^ bytes2[i % len(bytes2)] # ' ^ ' is the XOR symbol in python, I used mod to make sure that the password array wraps around to the start again if the pickled password array is longer (which is almost certain)
    return bytes1 #Returns the XOR'd pickled password dict
    

menuChoices = [openFile, createFile] #These two lists are the lists of functions I used for the menus.
fileChoices = [randPass, manPass, delPass]

print("Welcome to a password manager")
print("Main Menu")
print("1. Open an existing password file. \n2. Create a new file.")
action = int(input())

menuChoices[action - 1]() #The same function menu list thing as I did before