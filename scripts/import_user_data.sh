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

# --- User Data ---
owner_name=$(jq -r '.owner.name' "$json_file")
display_name_json=$(jq -r '.displayName' "$json_file")
if [[ "$display_name_json" == "null" ]] || [[ "$(jq -r '.Case' <<< "$display_name_json")" == "None" ]]; then
    display_name="NULL"
else
    display_name="'$(jq -r '.displayName.Fields[0]' "$json_file")'"
fi

display_type=$(jq -r '.displayType.Case' "$json_file")
latest_period=$(jq -r '.calendars | keys | map(tonumber) | max' "$json_file")

echo "INSERT INTO Users (Name, DisplayName, DisplayType, LatestPeriod) VALUES ('$owner_name', $display_name, '$display_type', $latest_period);"

# --- Calendar and Door Data ---
# Iterate over each calendar (period)
for period in $(jq -r '.calendars | keys | .[]' "$json_file"); do
    calendar_path=".calendars[\"$period\"]"

    # --- Calendar Data ---
    distance_factor=$(jq -r "$calendar_path.settings.distanceFactor" "$json_file")
    shared_link_id_json=$(jq -r "$calendar_path.settings.sharedLinkId" "$json_file")
    if [[ "$shared_link_id_json" == "null" ]]; then
        shared_link_id="NULL"
    else
        shared_link_id_case=$(printf '%s\n' "$shared_link_id_json" | jq '.Case' -r)
#        shared_link_id_case=$(cat "$shared_link_id_json" | jq -r ".Case")
        if [[ "$shared_link_id_case" == "Some" ]]; then
            shared_link_id=$(printf '%s\n' "$shared_link_id_json" | jq '.Fields[0]' -r)
            shared_link_id="'$shared_link_id'"
        else
            shared_link_id="NULL"
        fi
    fi

    verified_distance_json=$(jq -r "$calendar_path.verifiedDistance" "$json_file")
    if [[ "$verified_distance_json" == "null" ]] || [[ "$(jq -r '.Case' <<< "$verified_distance_json")" == "None" ]]; then
        verified_distance="NULL"
    else
        verified_distance=$(jq -r '.Fields[0]' <<< "$verified_distance_json")
    fi

    echo "INSERT INTO Calendars (OwnerName, Period, DistanceFactor, SharedLinkId, VerifiedDistance) VALUES ('$owner_name', $period, $distance_factor, $shared_link_id, $verified_distance);"

    # --- Door Data ---
    # Iterate over each door in the current calendar
    jq -c "$calendar_path.doors[]" "$json_file" | while read -r door; do
        day=$(jq -r '.day' <<< "$door")
        distance=$(jq -r '.distance' <<< "$door")
        state=$(jq -r '.state.Case' <<< "$door")

        echo "INSERT INTO Doors (OwnerName, Period, Day, Distance, State) VALUES ('$owner_name', $period, $day, $distance, '$state');"
    done
done
