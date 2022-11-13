FROM node:18.12.1
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm install
COPY . .
RUN npm run build
CMD ["npm", "start", "--", "--host", "0.0.0.0"]
