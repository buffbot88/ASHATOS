#!/bin/bash

echo "=========================================="
echo "ASHATAIServer Integration Test"
echo "GameServer + AI Services"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counters
TESTS_PASSED=0
TESTS_FAILED=0

# Helper function for test results
test_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
        ((TESTS_PASSED++))
    else
        echo -e "${RED}✗${NC} $2"
        ((TESTS_FAILED++))
    fi
}

# Build the project
echo "1. Building ASHATAIServer..."
cd /home/runner/work/ASHATOS/ASHATOS/AGP_AI/ASHATAIServer
BUILD_OUTPUT=$(dotnet build 2>&1)
BUILD_STATUS=$?
test_result $BUILD_STATUS "ASHATAIServer build"

if [ $BUILD_STATUS -ne 0 ]; then
    echo "Build failed, aborting tests"
    echo "$BUILD_OUTPUT"
    exit 1
fi

echo ""
echo "2. Starting ASHATAIServer..."
dotnet run --no-build > /tmp/ashatai_test.log 2>&1 &
SERVER_PID=$!
echo "   Server PID: $SERVER_PID"

# Wait for server to start
sleep 5

# Check if server is running
if ps -p $SERVER_PID > /dev/null; then
    test_result 0 "Server started successfully"
else
    test_result 1 "Server failed to start"
    cat /tmp/ashatai_test.log
    exit 1
fi

echo ""
echo "3. Testing AI Services..."

# Test AI Health
HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" http://localhost:8088/api/ai/health)
if [ "$HTTP_CODE" == "200" ]; then
    STATUS=$(jq -r '.status' /tmp/response.json)
    if [ "$STATUS" == "healthy" ]; then
        test_result 0 "AI health check endpoint"
    else
        test_result 1 "AI health check status"
    fi
else
    test_result 1 "AI health check endpoint (HTTP $HTTP_CODE)"
fi

# Test AI Status
HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" http://localhost:8088/api/ai/status)
if [ "$HTTP_CODE" == "200" ]; then
    test_result 0 "AI status endpoint"
else
    test_result 1 "AI status endpoint (HTTP $HTTP_CODE)"
fi

echo ""
echo "4. Testing Game Server Services..."

# Test Game Server Status
HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" http://localhost:8088/api/gameserver/status)
if [ "$HTTP_CODE" == "200" ]; then
    test_result 0 "Game server status endpoint"
else
    test_result 1 "Game server status endpoint (HTTP $HTTP_CODE)"
fi

# Test Game Server Capabilities
HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" http://localhost:8088/api/gameserver/capabilities)
if [ "$HTTP_CODE" == "200" ]; then
    MAX_SERVERS=$(jq -r '.maxConcurrentServers' /tmp/response.json)
    if [ "$MAX_SERVERS" == "50" ]; then
        test_result 0 "Game server capabilities endpoint"
    else
        test_result 1 "Game server capabilities data"
    fi
else
    test_result 1 "Game server capabilities endpoint (HTTP $HTTP_CODE)"
fi

echo ""
echo "5. Testing Game Creation..."

# Test AI-Enhanced Game Creation
HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" -X POST http://localhost:8088/api/gameserver/create-ai-enhanced \
  -H "Content-Type: application/json" \
  -d '{"description": "A medieval RPG with quests and magic", "userId": "00000000-0000-0000-0000-000000000000", "licenseKey": "TEST"}')

if [ "$HTTP_CODE" == "200" ]; then
    SUCCESS=$(jq -r '.success' /tmp/response.json)
    GAME_ID=$(jq -r '.gameId' /tmp/response.json)
    if [ "$SUCCESS" == "true" ] && [ -n "$GAME_ID" ] && [ "$GAME_ID" != "null" ]; then
        test_result 0 "AI-enhanced game creation"
        echo "   Created game ID: $GAME_ID"
    else
        test_result 1 "AI-enhanced game creation (success=$SUCCESS, id=$GAME_ID)"
    fi
else
    test_result 1 "AI-enhanced game creation (HTTP $HTTP_CODE)"
    cat /tmp/response.json
fi

# Test Regular Game Creation
HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" -X POST http://localhost:8088/api/gameserver/create \
  -H "Content-Type: application/json" \
  -d '{"userId": "00000000-0000-0000-0000-000000000000", "description": "A space shooter", "gameType": 2, "theme": "sci-fi", "features": ["Combat System"], "licenseKey": "TEST", "generateAssets": true}')

if [ "$HTTP_CODE" == "200" ]; then
    SUCCESS=$(jq -r '.success' /tmp/response.json)
    GAME_ID2=$(jq -r '.gameId' /tmp/response.json)
    if [ "$SUCCESS" == "true" ] && [ -n "$GAME_ID2" ] && [ "$GAME_ID2" != "null" ]; then
        test_result 0 "Regular game creation"
        echo "   Created game ID: $GAME_ID2"
    else
        test_result 1 "Regular game creation"
    fi
else
    test_result 1 "Regular game creation (HTTP $HTTP_CODE)"
fi

echo ""
echo "6. Testing Game Management..."

# Test List Projects
HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" http://localhost:8088/api/gameserver/projects)
if [ "$HTTP_CODE" == "200" ]; then
    PROJECTS=$(jq -r '.projects' /tmp/response.json)
    if [[ "$PROJECTS" == *"Game Projects"* ]]; then
        test_result 0 "List game projects"
    else
        test_result 1 "List game projects (empty or invalid)"
    fi
else
    test_result 1 "List game projects (HTTP $HTTP_CODE)"
fi

# Test Get Project Details (if we have a game ID)
if [ -n "$GAME_ID" ] && [ "$GAME_ID" != "null" ]; then
    HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" http://localhost:8088/api/gameserver/project/$GAME_ID)
    if [ "$HTTP_CODE" == "200" ]; then
        PROJECT_ID=$(jq -r '.id' /tmp/response.json)
        if [ "$PROJECT_ID" == "$GAME_ID" ]; then
            test_result 0 "Get project details"
        else
            test_result 1 "Get project details (ID mismatch)"
        fi
    else
        test_result 1 "Get project details (HTTP $HTTP_CODE)"
    fi
fi

echo ""
echo "7. Testing AI Integration..."

# Test AI Suggestions (if we have a game ID)
if [ -n "$GAME_ID" ] && [ "$GAME_ID" != "null" ]; then
    HTTP_CODE=$(curl -s -o /tmp/response.json -w "%{http_code}" http://localhost:8088/api/gameserver/suggestions/$GAME_ID)
    if [ "$HTTP_CODE" == "200" ]; then
        SUGGESTIONS=$(jq -r '.suggestions' /tmp/response.json)
        if [ -n "$SUGGESTIONS" ] && [ "$SUGGESTIONS" != "null" ]; then
            test_result 0 "AI suggestions endpoint"
            echo "   Suggestion sample: ${SUGGESTIONS:0:60}..."
        else
            test_result 1 "AI suggestions (empty response)"
        fi
    else
        test_result 1 "AI suggestions (HTTP $HTTP_CODE)"
    fi
fi

echo ""
echo "8. Cleanup..."

# Stop the server
kill $SERVER_PID 2>/dev/null
wait $SERVER_PID 2>/dev/null
test_result 0 "Server stopped gracefully"

echo ""
echo "=========================================="
echo "Test Summary"
echo "=========================================="
echo -e "Tests Passed: ${GREEN}$TESTS_PASSED${NC}"
echo -e "Tests Failed: ${RED}$TESTS_FAILED${NC}"
echo "Total Tests: $((TESTS_PASSED + TESTS_FAILED))"
echo ""

if [ $TESTS_FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ All tests passed!${NC}"
    exit 0
else
    echo -e "${RED}✗ Some tests failed${NC}"
    echo "See log: /tmp/ashatai_test.log"
    exit 1
fi
