#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Starting E2E tests with Aspire...${NC}"

# Function to cleanup on exit
cleanup() {
    echo -e "${YELLOW}🧹 Cleaning up...${NC}"
    if [ ! -z "$ASPIRE_PID" ]; then
        echo "Stopping Aspire (PID: $ASPIRE_PID)"
        kill $ASPIRE_PID 2>/dev/null || true
    fi
    exit $1
}

# Set trap to cleanup on script exit
trap 'cleanup $?' EXIT INT TERM

# Navigate to project root
cd "$(dirname "$0")/../../" || exit 1

# Build the solution
echo -e "${YELLOW}📦 Building solution...${NC}"
dotnet build --configuration Release || {
    echo -e "${RED}❌ Failed to build solution${NC}"
    exit 1
}

# Start Aspire AppHost in the background
echo -e "${YELLOW}🔧 Starting Aspire AppHost...${NC}"
dotnet run --project PrivateSocial.AppHost --configuration Release --no-build > aspire.log 2>&1 &
ASPIRE_PID=$!

echo "Aspire started with PID: $ASPIRE_PID"

# Wait for services to be ready
echo -e "${YELLOW}⏳ Waiting for services to be ready...${NC}"
MAX_ATTEMPTS=60
ATTEMPT=0

while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    ATTEMPT=$((ATTEMPT + 1))
    
    # Check if API is ready
    if curl -f http://localhost:5475/health 2>/dev/null; then
        echo -e "${GREEN}✅ API is ready!${NC}"
        
        # Also check if frontend is ready
        if curl -f http://localhost:3000 2>/dev/null; then
            echo -e "${GREEN}✅ Frontend is ready!${NC}"
            break
        fi
    fi
    
    echo "Attempt $ATTEMPT/$MAX_ATTEMPTS: Services not ready yet..."
    
    # Check if Aspire process is still running
    if ! ps -p $ASPIRE_PID > /dev/null; then
        echo -e "${RED}❌ Aspire process died. Last log output:${NC}"
        tail -n 50 aspire.log
        exit 1
    fi
    
    sleep 2
done

if [ $ATTEMPT -eq $MAX_ATTEMPTS ]; then
    echo -e "${RED}❌ Services failed to start within timeout${NC}"
    echo "Last log output:"
    tail -n 50 aspire.log
    exit 1
fi

# Wait a bit more for everything to stabilize
sleep 5

# Run E2E tests (don't start another server since Aspire already started one)
echo -e "${BLUE}🎭 Running E2E tests...${NC}"
cd PrivateSocial.Web.React

# Run without starting server since Aspire already started it
npx playwright test

TEST_EXIT_CODE=$?

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✅ All tests passed!${NC}"
else
    echo -e "${RED}❌ Tests failed with exit code $TEST_EXIT_CODE${NC}"
fi

exit $TEST_EXIT_CODE