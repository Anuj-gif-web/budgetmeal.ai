# backend/main.py

from fastapi import FastAPI

app = FastAPI()

@app.get("/")
def read_root():
    return {"message": "BudgetMeal.AI backend is live"}
