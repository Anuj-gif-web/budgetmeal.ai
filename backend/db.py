from pymongo import MongoClient
import os
from dotenv import load_dotenv

# Load .env file for MONGO_URI
load_dotenv()

MONGO_URI = os.getenv("MONGO_URI")
client = MongoClient(MONGO_URI)

# Use your DB name here
db = client["budgetmeal"]

# Generic function to get any collection
def get_collection(name):
    return db[name]
