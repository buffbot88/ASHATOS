#!/bin/bash
# Example: Using Ashat to create a new module
# This demonstRates a complete session workflow

echo "========================================="
echo "Ashat Example: Creating a New Module"
echo "========================================="
echo ""

# Simple Demonstration of Ashat commands
echo "This example shows how to use Ashat to create a module."
echo ""

USER_ID="demo_user"
GOAL="Create a simple notification module"

echo "1. Starting a session:"
echo "   Command: ashat start session $USER_ID $GOAL"
echo ""

echo "2. Continue the conversation:"
echo "   Command: ashat continue $USER_ID It should send email and SMS notifications"
echo ""

echo "3. Review the action plan and approve:"
echo "   Command: ashat approve $USER_ID"
echo ""

echo "4. End the session:"
echo "   Command: ashat end session $USER_ID"
echo ""

echo "========================================="
echo "Try it yourself!"
echo "========================================="
echo ""
echo "Run from ASHATCore directory:"
echo "  dotnet run -- 'ashat start session YOUR_USER_ID YOUR_GOAL'"
echo ""
