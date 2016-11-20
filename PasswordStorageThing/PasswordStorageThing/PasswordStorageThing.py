import pickle
import os
import os.path
import hashlib
import string
import random


passwords = {}
chars = string.ascii_letters + string.digits


def openFile():
    filename = input("Enter the name of the file you want to open: ")
    if os.path.exists(filename) == False:
        print("This file does not exist")
        return
    file = open(filename, 'r+b')
    encryptedPasses = file.read()
    password = input("Enter the master password for this file: ")
    hashedMaster = hashlib.sha1(password)
    decryptedPasses = logicalXOR(encryptedPasses, hashedMaster)
    passwords = pickle.load(encryptedPasses)
    print(passwords)


def createFile():
    filename = input("Enter the name of the file you want to make: ")
    filepass = input("Enter the desired master password for this file: ")
    if os.path.exists(filename):
        print("This file already exists, cancelling operation.")
        return    
    while True:
        print("1. Generate a random password \n2. Manually add a password. \n3. Finish")
        action = int(input())
        if action == 3:
            break
        fileChoices[action - 1]()

    pickledPasses = pickle.dumps(passwords)
    hashedMaster = hashlib.sha1(filepass.encode(encoding="UTF-8")).digest()
    file = open(filename+".pass", 'wb')
    file.write(logicalXOR(pickledPasses, hashedMaster))


def randPass():
    service = input("This password is for: ")
    passLen = int(input("Enter the length you want for the password: "))
    random.seed = os.urandom(1024)
    randomPass = ''.join(random.choice(chars) for i in range(passLen))
    print("The password is: "+randomPass)
    passwords[service] = randomPass


def manPass():
    service = input("This password is for: ")
    password = input("Enter the password: ")
    passwords[service] = password

def logicalXOR(passes, masterpass):
    bytes1 = bytearray(passes)
    bytes2 = bytearray(masterpass)
    for i in range(0, len(bytes1)-1):
        bytes1[i] = bytes1[i] ^ bytes2[i % len(bytes2)]
    return bytes1
    

menuChoices = [openFile, createFile]
fileChoices = [randPass, manPass]

print("Welcome to a password manager")
print("Main Menu")
print("1. Open an existing password file. \n2. Create a new file.")
action = int(input())

menuChoices[action - 1]()