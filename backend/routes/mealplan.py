from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from typing import List
import google.generativeai as genai
import os
from dotenv import load_dotenv
import json

# Load environment variables
load_dotenv()

# Configure Gemini API
api_key = os.getenv("GEMINI_API_KEY")
if not api_key:
    raise ValueError("GEMINI_API_KEY not found in environment")
genai.configure(api_key=api_key)

router = APIRouter()

# Request and response models
class MealPlanRequest(BaseModel):
    budget: float
    diet: str
    pantry: List[str]

class Recipe(BaseModel):
    title: str
    ingredients: List[str]
    cost: float

@router.post("/generate-meal-plan", response_model=List[Recipe])
def generate_meal_plan(request: MealPlanRequest):
    model = genai.GenerativeModel("models/gemini-1.5-flash")  

    # Prompt to guide Gemini
    prompt = (
        f"Create 14 budget-friendly {request.diet} recipes using only these pantry items: {', '.join(request.pantry)}.\n"
        f"Each recipe must include: a title, ingredients used (from pantry), and an estimated cost (under ${request.budget}).\n"
        f"Respond strictly in raw JSON array format (no markdown, no explanation), like:\n"
        f'[{{"title": "...", "ingredients": [...], "cost": ...}}, ...]'
    )

    try:
        response = model.generate_content(prompt)
        json_str = response.text.strip()

        # Handle wrapped code block if returned
        if json_str.startswith("```json"):
            json_str = json_str.strip("```json").strip("```").strip()

        # Parse JSON safely
        recipes_data = json.loads(json_str)

        return [Recipe(**r) for r in recipes_data]

    except json.JSONDecodeError as e:
        raise HTTPException(status_code=500, detail=f"Invalid JSON from Gemini: {str(e)}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
