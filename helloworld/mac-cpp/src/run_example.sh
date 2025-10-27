#!/bin/bash

# Ensure the script exits if any command fails
set -e

# --- Configuration ---
SRC_DIR="." # Assuming the script is run from the src directory
SDK_DIR="sdk"
LICENSE_DIR="license"
FRAMEWORKS_DIR="Frameworks"
EXCLUDE_DIRS=("SDKDownloader" "sdk" "license" "Frameworks" ".") # Directories to exclude when detecting examples

# --- Detect Examples ---
EXAMPLES=()
for dir in "$SRC_DIR"/*/; do
    # Remove trailing slash
    dir_name=$(basename "$dir")
    is_excluded=false
    for excluded in "${EXCLUDE_DIRS[@]}"; do
        if [[ "$dir_name" == "$excluded" ]]; then
            is_excluded=true
            break
        fi
    done
    if ! $is_excluded && [ -f "$dir/Makefile" ]; then # Check if it's a directory and contains a Makefile
        EXAMPLES+=("$dir_name")
    fi
done

# --- Helper Function: Print Usage ---
print_usage() {
    echo "Usage: $0 <ExampleName>"
    echo "Builds and runs the specified example."
    echo ""
    echo "Available examples:"
    for example in "${EXAMPLES[@]}"; do
        echo "  - $example"
    done
    exit 1
}

# --- Check Input ---
if [ "$#" -ne 1 ]; then
    echo "Error: Please provide the name of the example to run."
    print_usage
fi

TARGET_EXAMPLE=$1
EXAMPLE_FOUND=false
for example in "${EXAMPLES[@]}"; do
    if [[ "$TARGET_EXAMPLE" == "$example" ]]; then
        EXAMPLE_FOUND=true
        break
    fi
done

if ! $EXAMPLE_FOUND; then
    echo "Error: Example '$TARGET_EXAMPLE' not found or is not a valid example directory."
    print_usage
fi

EXAMPLE_DIR="$SRC_DIR/$TARGET_EXAMPLE"
EXECUTABLE_NAME=$(basename "$TARGET_EXAMPLE") # Assuming executable name matches dir name

# --- Check Prerequisites ---
if [ ! -d "$SRC_DIR/$SDK_DIR" ]; then
    echo "Error: SDK directory '$SRC_DIR/$SDK_DIR' not found."
    echo "Please run 'build_sdk_downloader.sh' first to download the SDK."
    exit 1
fi

if [ ! -d "$SRC_DIR/$LICENSE_DIR" ] || [ ! -e "$SRC_DIR/$LICENSE_DIR/license.cfg" ]; then
    echo "Error: License directory '$SRC_DIR/$LICENSE_DIR' or license file 'license.cfg' not found."
    echo "Please ensure the license files are present in the '$SRC_DIR/$LICENSE_DIR' directory."
    exit 1
fi


echo "--- Preparing Example: $TARGET_EXAMPLE ---"

# --- Copy License Files ---
echo "Copying license files to $EXAMPLE_DIR..."
# Copy license files directly to the example's root directory (not to a license subdirectory)
cp "$SRC_DIR/$LICENSE_DIR/license.cfg" "$EXAMPLE_DIR/"
cp "$SRC_DIR/$LICENSE_DIR/pass_key.txt" "$EXAMPLE_DIR/"

# Also copy to license dir for backward compatibility if needed
mkdir -p "$EXAMPLE_DIR/$LICENSE_DIR"
cp -R "$SRC_DIR/$LICENSE_DIR/"* "$EXAMPLE_DIR/$LICENSE_DIR/"


echo "--- Building Example: $TARGET_EXAMPLE ---"
cd "$EXAMPLE_DIR"

echo "Running 'make clean'..."
if ! make clean; then
    echo "Error: 'make clean' failed in $EXAMPLE_DIR directory."
    cd "$SRC_DIR" # Go back to src dir before exiting
    exit 1
fi

echo "Running 'make'..."
if ! make; then
    echo "Error: 'make' failed in $EXAMPLE_DIR directory."
    cd "$SRC_DIR" # Go back to src dir before exiting
    exit 1
fi

# First check if we can determine the executable name from the Makefile
OUT_VAR=$(grep "OUT = " Makefile | sed 's/OUT = //')
if [ -n "$OUT_VAR" ] && [ -f "./$OUT_VAR" ]; then
    FINAL_EXECUTABLE="./$OUT_VAR"
    echo "Found executable from Makefile: $FINAL_EXECUTABLE"
else
    # Try to find the executable using various naming conventions
    EXECUTABLE_PATH="./$EXECUTABLE_NAME"
    LOWERCASE_EXECUTABLE_PATH="./$(echo $EXECUTABLE_NAME | tr '[:upper:]' '[:lower:]')"
    HYPHENATED_EXECUTABLE_PATH="./$(echo $EXECUTABLE_NAME | tr '[:upper:]' '[:lower:]' | tr ' ' '-')"
    UNDERSCORED_EXECUTABLE_PATH="./$(echo $EXECUTABLE_NAME | tr '[:upper:]' '[:lower:]' | tr ' ' '_')"

    FINAL_EXECUTABLE=""
    if [ -f "$EXECUTABLE_PATH" ] && [ -x "$EXECUTABLE_PATH" ]; then
        FINAL_EXECUTABLE="$EXECUTABLE_PATH"
    elif [ -f "$LOWERCASE_EXECUTABLE_PATH" ] && [ -x "$LOWERCASE_EXECUTABLE_PATH" ]; then
        FINAL_EXECUTABLE="$LOWERCASE_EXECUTABLE_PATH"
    elif [ -f "$HYPHENATED_EXECUTABLE_PATH" ] && [ -x "$HYPHENATED_EXECUTABLE_PATH" ]; then
        FINAL_EXECUTABLE="$HYPHENATED_EXECUTABLE_PATH"
    elif [ -f "$UNDERSCORED_EXECUTABLE_PATH" ] && [ -x "$UNDERSCORED_EXECUTABLE_PATH" ]; then
        FINAL_EXECUTABLE="$UNDERSCORED_EXECUTABLE_PATH"
    else
        # If we can't find the executable using naming patterns, try using find to locate non-library executables
        echo "Looking for executable files (excluding libraries)..."
        EXECUTABLES=$(find . -type f -perm -u+x -not -name "*.dylib" -not -name "*.so" -not -path "*/\.*" | head -1)
        if [ -n "$EXECUTABLES" ]; then
            FINAL_EXECUTABLE="$EXECUTABLES"
            echo "Found executable: $FINAL_EXECUTABLE"
        else
            echo "Error: Could not find any executable in the current directory. Check the Makefile."
            cd "$SRC_DIR" # Go back to src dir before exiting
            exit 1
        fi
    fi
fi

echo "--- Running Example: $TARGET_EXAMPLE ---"
echo "Executing '$FINAL_EXECUTABLE'..."
if ! "$FINAL_EXECUTABLE"; then
     echo "Error: Failed to run the example '$TARGET_EXAMPLE'."
     cd "$SRC_DIR" # Go back to src dir before exiting
     exit 1
fi

cd "$SRC_DIR" # Go back to the src directory

echo "--- Example $TARGET_EXAMPLE finished successfully! ---"

# Note: We do not remove the Frameworks directory as it may be needed by other examples
# If you want to clean up completely, run 'make clean' in the example directory

exit 0 