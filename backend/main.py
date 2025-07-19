# backend/main.py
import os
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv
from routes import mealplan  # This is your Gemini route file

load_dotenv()  # Loads GEMINI_API_KEY from .env
print("Gemini key:", os.getenv("GEMINI_API_KEY")) 

app = FastAPI()

# CORS config - adjust the origin to your frontend URL in prod
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # or ["http://localhost:5173"] etc.
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/")
def read_root():
    return {"message": "BudgetMeal.AI backend is live"}

# Include Gemini meal plan route
app.include_router(mealplan.router)
