#!/usr/bin/env node

const { spawn } = require('child_process');
const path = require('path');
const waitOn = require('wait-on');

// Colors for console output
const colors = {
  reset: '\x1b[0m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m'
};

function log(message, color = 'reset') {
  console.log(`${colors[color]}${message}${colors.reset}`);
}

async function runE2ETests() {
  const projectRoot = path.resolve(__dirname, '../../');
  const appHostPath = path.join(projectRoot, 'PrivateSocial.AppHost');
  
  log('🚀 Starting E2E test suite...', 'blue');
  
  // Start the backend
  log('📦 Starting backend services...', 'yellow');
  const backend = spawn('dotnet', ['run', '--project', appHostPath], {
    cwd: projectRoot,
    stdio: 'pipe',
    shell: true
  });

  let backendStarted = false;

  // Capture backend output
  backend.stdout.on('data', (data) => {
    const output = data.toString();
    if (!backendStarted) {
      console.log(`[Backend] ${output}`);
      if (output.includes('Aspire dashboard') || output.includes('Now listening on')) {
        backendStarted = true;
      }
    }
  });

  backend.stderr.on('data', (data) => {
    console.error(`[Backend Error] ${data}`);
  });

  backend.on('error', (error) => {
    log(`Failed to start backend: ${error.message}`, 'red');
    process.exit(1);
  });

  // Wait for backend to be ready
  log('⏳ Waiting for backend to be ready...', 'yellow');
  
  try {
    await waitOn({
      resources: [
        'https://localhost:17253', // Aspire dashboard
        'http://localhost:5475/health', // API service
        'http://localhost:3000' // Frontend
      ],
      timeout: 90000, // 90 seconds
      interval: 2000,
      validateStatus: function (status) {
        return status >= 200 && status < 500; // Accept redirects and such
      },
      strictSSL: false // For local HTTPS
    });
    
    log('✅ Backend is ready!', 'green');
  } catch (error) {
    log('❌ Backend failed to start within timeout', 'red');
    backend.kill();
    process.exit(1);
  }

  // Run the Playwright tests
  log('🎭 Running Playwright tests...', 'blue');
  const tests = spawn('npm', ['run', 'playwright', 'test'], {
    cwd: path.resolve(__dirname, '..'),
    stdio: 'inherit',
    shell: true
  });

  tests.on('close', (code) => {
    // Clean up
    log('🧹 Cleaning up...', 'yellow');
    backend.kill();
    
    if (code === 0) {
      log('✅ All tests passed!', 'green');
    } else {
      log(`❌ Tests failed with exit code ${code}`, 'red');
    }
    
    process.exit(code);
  });

  // Handle interruption
  process.on('SIGINT', () => {
    log('\n🛑 Test run interrupted, cleaning up...', 'yellow');
    backend.kill();
    tests.kill();
    process.exit(1);
  });
}

// Run the tests
runE2ETests().catch((error) => {
  log(`Unexpected error: ${error.message}`, 'red');
  process.exit(1);
});