#!/bin/bash

# Test script for ASHAT Headless Host

echo "================================"
echo "ASHAT Headless Host Test Suite"
echo "================================"
echo ""

# Build the project
echo "Building project..."
dotnet build > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi
echo "✓ Build successful"
echo ""

# Test 1: Headless mode starts
echo "Test 1: Starting headless mode..."
cd bin/Debug/net9.0
timeout 3 bash -c 'echo "exit" | ./ASHAT --headless' > /tmp/test1.log 2>&1
if grep -q "ASHAT Headless Host Service started successfully" /tmp/test1.log; then
    echo "✓ Headless mode starts successfully"
else
    echo "❌ Headless mode failed to start"
    cat /tmp/test1.log
    exit 1
fi
echo ""

# Test 2: Configuration loading
echo "Test 2: Configuration loading..."
if grep -q "Server: http://agpstudios.online" /tmp/test1.log; then
    echo "✓ Configuration loaded correctly"
else
    echo "❌ Configuration not loaded"
    exit 1
fi
echo ""

# Test 3: Session creation
echo "Test 3: Session management..."
if grep -q "Created new session:" /tmp/test1.log; then
    echo "✓ Session created successfully"
else
    echo "❌ Session creation failed"
    exit 1
fi
echo ""

# Test 4: Message processing
echo "Test 4: Message processing..."
timeout 5 bash -c 'echo -e "hello\nexit" | ./ASHAT --headless' > /tmp/test2.log 2>&1
if grep -q "Processing message: hello" /tmp/test2.log; then
    echo "✓ Message processing works"
else
    echo "❌ Message processing failed"
    cat /tmp/test2.log
    exit 1
fi
echo ""

# Test 5: Fallback mode
echo "Test 5: Fallback mode (when server unavailable)..."
if grep -q "Using fallback response" /tmp/test2.log; then
    echo "✓ Fallback mode active"
else
    echo "❌ Fallback mode not working"
    exit 1
fi
echo ""

# Test 6: Consent system
echo "Test 6: Consent system..."
timeout 5 bash -c 'echo -e "consent\nexit" | ./ASHAT --headless' > /tmp/test3.log 2>&1
if grep -q "Persistent memory enabled" /tmp/test3.log; then
    echo "✓ Consent system works"
else
    echo "❌ Consent system failed"
    cat /tmp/test3.log
    exit 1
fi
echo ""

# Test 7: Graceful shutdown
echo "Test 7: Graceful shutdown..."
if grep -q "Service stopped" /tmp/test1.log; then
    echo "✓ Graceful shutdown works"
else
    echo "❌ Shutdown failed"
    exit 1
fi
echo ""

cd ../../..

echo "================================"
echo "All tests passed! ✓"
echo "================================"
