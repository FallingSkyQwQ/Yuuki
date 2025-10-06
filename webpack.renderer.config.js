const rules = require('./webpack.rules');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');

rules.push({
  test: /\.css$/,
  use: [
    'style-loader',
    'css-loader',
  ],
});

module.exports = {
  module: {
    rules,
  },
  plugins: [
    new HtmlWebpackPlugin({
      template: './src/renderer/index.html',
      inject: 'body',
    }),
    new ForkTsCheckerWebpackPlugin({
      typescript: {
        configFile: './tsconfig.json',
      },
    }),
  ],
  resolve: {
    extensions: ['.js', '.ts', '.jsx', '.tsx', '.css', '.json'],
  },
};