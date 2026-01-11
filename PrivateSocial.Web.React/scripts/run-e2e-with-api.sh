#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Starting E2E tests with API...${NC}"

# Set environment variables for the API
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:5475"
export ConnectionStrings__privatesocial="Server=localhost,1433;Database=PrivateSocial;User Id=sa;Password=TestP@ssw0rd123!;TrustServerCertificate=True"
export ConnectionStrings__redis="localhost:6379"
export Jwt__Secret="TestSecretKeyForE2ETestingPurposesOnly123456789"
export Jwt__Issuer="PrivateSocial"
export Jwt__Audience="PrivateSocialUsers"
export ASPNETCORE_CORS_ORIGINS="http://localhost:3000"

# Function to cleanup on exit
cleanup() {
    echo -e "${YELLOW}🧹 Cleaning up...${NC}"
    if [ ! -z "$API_PID" ]; then
        echo "Stopping API (PID: $API_PID)"
        kill $API_PID 2>/dev/null || true
    fi
    exit $1
}

# Set trap to cleanup on script exit
trap 'cleanup $?' EXIT INT TERM

# Navigate to project root
cd "$(dirname "$0")/../../" || exit 1

# Build the API
echo -e "${YELLOW}📦 Building API...${NC}"
dotnet build PrivateSocial.ApiService/PrivateSocial.ApiService.csproj --configuration Release || {
    echo -e "${RED}❌ Failed to build API${NC}"
    exit 1
}

# Start the API in the background
echo -e "${YELLOW}🔧 Starting API service...${NC}"
cd PrivateSocial.ApiService
dotnet run --configuration Release --no-build > ../api.log 2>&1 &
API_PID=$!
cd ..

echo "API started with PID: $API_PID"

# Wait for API to be ready
echo -e "${YELLOW}⏳ Waiting for API to be ready...${NC}"
MAX_ATTEMPTS=30
ATTEMPT=0

while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    ATTEMPT=$((ATTEMPT + 1))
    
    if curl -f http://localhost:5475/health 2>/dev/null; then
        echo -e "${GREEN}✅ API is ready!${NC}"
        break
    fi
    
    echo "Attempt $ATTEMPT/$MAX_ATTEMPTS: API not ready yet..."
    
    # Check if API process is still running
    if ! ps -p $API_PID > /dev/null; then
        echo -e "${RED}❌ API process died. Last log output:${NC}"
        tail -n 50 api.log
        exit 1
    fi
    
    sleep 2
done

if [ $ATTEMPT -eq $MAX_ATTEMPTS ]; then
    echo -e "${RED}❌ API failed to start within timeout${NC}"
    echo "Last log output:"
    tail -n 50 api.log
    exit 1
fi

# Export environment variables for Vite
export services__apiservice__http__0="http://localhost:5475"
export services__apiservice__https__0="http://localhost:5475"
export VITE_API_URL="http://localhost:5475"

# Run E2E tests
echo -e "${BLUE}🎭 Running E2E tests...${NC}"
cd PrivateSocial.Web.React
npm run test:e2e

TEST_EXIT_CODE=$?

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✅ All tests passed!${NC}"
else
    echo -e "${RED}❌ Tests failed with exit code $TEST_EXIT_CODE${NC}"
fi

exit $TEST_EXIT_CODE