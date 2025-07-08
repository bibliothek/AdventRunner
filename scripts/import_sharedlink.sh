#!/bin/bash

# Check for correct number of arguments
if [ "$#" -ne 1 ]; then
    echo "Usage: $0 <json_file>"
    exit 1
fi

# Read the JSON file from the first argument
json_file="$1"

# Check if the file exists
if [ ! -f "$json_file" ]; then
    echo "Error: File not found: $json_file"
    exit 1
fi

# Check if jq is installed
if ! command -v jq &> /dev/null
then
    echo "jq could not be found. Please install jq to run this script."
    exit
fi

# Parse the JSON and extract the values
id=$(jq -r '.id' "$json_file")
owner_name=$(jq -r '.owner.name' "$json_file")
period=$(jq -r '.period' "$json_file")

# Create the SQL INSERT statement
sql_statement="INSERT INTO SharedLinks (Id, OwnerName, Period) VALUES ('$id', '$owner_name', $period);"

# Print the SQL statement
echo "$sql_statement"
