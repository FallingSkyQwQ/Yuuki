import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import './index.css';

// Get the root element
const container = document.getElementById('root');
if (!container) {
  throw new Error('Root element not found');
}

// Create the root and render the app
const root = createRoot(container);
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

// Handle HMR (Hot Module Replacement)
// @ts-ignore
if (module.hot) {
  // @ts-ignore
  module.hot.accept();
}