from fastapi import APIRouter
from db import get_collection

test_router = APIRouter()

@test_router.get("/ping-db")
def test_db():
    users = get_collection("users")
    return {"count": users.count_documents({})}
