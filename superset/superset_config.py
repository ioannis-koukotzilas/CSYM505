import os

SECRET_KEY = os.environ["SUPERSET_SECRET_KEY"]

SQLALCHEMY_DATABASE_URI = "sqlite:////app/superset_home/superset.db"

# Allow the local SQLite data source.
PREVENT_UNSAFE_DB_CONNECTIONS = False