#!/bin/bash

# Test script for ASHAT GUI Mode

echo "================================"
echo "ASHAT GUI Mode Test Suite"
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

# Test 1: GUI mode starts
echo "Test 1: Starting GUI mode..."
timeout 7 xvfb-run -a dotnet run > /tmp/gui_test1.log 2>&1 &
GUI_PID=$!
sleep 5

# Check if process is still running
if ps -p $GUI_PID > /dev/null; then
    echo "✓ GUI mode started successfully"
    kill -TERM $GUI_PID 2>/dev/null
    wait $GUI_PID 2>/dev/null
else
    echo "❌ GUI mode crashed"
    cat /tmp/gui_test1.log
    exit 1
fi
echo ""

# Test 2: Animation system initialization
echo "Test 2: Animation system..."
if grep -q "Playing animation: greeting" /tmp/gui_test1.log; then
    echo "✓ Animation system initialized"
else
    echo "❌ Animation system failed"
    cat /tmp/gui_test1.log
    exit 1
fi
echo ""

# Test 3: TTS system
echo "Test 3: TTS system..."
if grep -q "ASHAT Speaking" /tmp/gui_test1.log; then
    echo "✓ TTS system active"
else
    echo "❌ TTS system not working"
    cat /tmp/gui_test1.log
    exit 1
fi
echo ""

# Test 4: Goddess initialization
echo "Test 4: Goddess initialization..."
if grep -q "Greetings, mortal!" /tmp/gui_test1.log; then
    echo "✓ Goddess initialized and greeting"
else
    echo "❌ Goddess initialization failed"
    cat /tmp/gui_test1.log
    exit 1
fi
echo ""

# Test 5: Animation states
echo "Test 5: Animation state transitions..."
if grep -q "Playing animation: idle" /tmp/gui_test1.log; then
    echo "✓ Animation states working"
else
    echo "❌ Animation state transitions failed"
    cat /tmp/gui_test1.log
    exit 1
fi
echo ""

echo "================================"
echo "All GUI tests passed! ✓"
echo "================================"
echo ""
echo "Note: These tests verify the GUI starts and initializes correctly."
echo "Visual appearance should be verified manually on a system with a display."
