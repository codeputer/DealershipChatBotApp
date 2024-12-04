(async function () {
  const webTokenUrl = "{webTokenUrl}"; // URL to fetch WebChat token
  const chatEndpointUrl = "{chatEndpointUrl}"; // URL for WebChat messages

  // Function to fetch a new WebToken
  async function fetchWebToken() {
    const response = await fetch(webTokenUrl, {
      method: "GET",
      headers: {
        Authorization: `Bearer {dealerJWTToken}`,
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) throw new Error("Failed to fetch WebToken");

    const data = await response.json();
    const newToken = data.jwttoken;

    // Save the token to localStorage
    localStorage.setItem("webToken", newToken);

    console.log("Fetched and saved new WebToken:", newToken);

    return newToken;
  }

  // Function to get a valid token (reuse or fetch new)
  async function getWebToken() {
    let token = localStorage.getItem("webToken");
    if (!token) {
      console.log("No token in storage. Fetching a new one...");
      token = await fetchWebToken();
    }
    return token;
  }

  // Function to send chat data
  async function sendChatData(questions) {
    let token = await getWebToken();

    const response = await fetch(chatEndpointUrl, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ questions }),
    });

    if (response.status === 401) {
      console.log("Token expired. Fetching a new one...");
      token = await fetchWebToken();
      return sendChatData(questions);
    }

    if (!response.ok) throw new Error("Failed to send chat data");

    return await response.json();
  }

  // Initialize the chat container
  const chatContainer = document.createElement("div");
  chatContainer.id = "chatContainer";
  chatContainer.innerHTML = `
    <div id="chatWindowContainer">
        <div id="chatWindow"></div> 
    </div>
    <div id="chatInputContainer">
        <input type="text" id="chatInput" placeholder="Type your question here..." />
        <button id="askButton">Ask</button>
    </div>
    <div id="statusMessage"></div>
  `;
  document.body.appendChild(chatContainer);

  // Add embedded CSS
  const style = document.createElement("style");
  style.textContent = `
        #chatContainer {
            width: 400px;
            margin: 20px auto;
            font-family: Arial, sans-serif;
        }
        #chatWindowContainer {
            border: 1px solid #ccc;
            margin-bottom: 10px;
            padding: 5px;
        }
        #chatInputContainer {
            display: flex;
            gap: 10px;
        }
        #chatInput {
            flex: 1;
            padding: 5px;
            font-size: 14px;
        }
        #askButton {
            padding: 5px 10px;
            font-size: 14px;
            cursor: pointer;
        }
        #statusMessage {
            font-size: 12px;
            color: #666;
            margin-top: 10px;
        }
        #chatWindow {
          width: 100%;
          height: 300px;
          overflow-y: auto;
          border: 1px solid #ccc;
          padding: 10px;
          font-family: Arial, sans-serif;
          font-size: 14px;
        }
        .message-container {
          margin-bottom: 10px;
        }
        .question {
          text-align: left;
          font-weight: bold;
          margin-bottom: 5px;
        }
        .answer {
          text-align: right;
          word-wrap: break-word;
          max-width: 50%;
          margin-left: auto;
        }
    `;
  document.head.appendChild(style);

  // Initialize questions array
  const questions = [
    { asked: "Hello?", answer: "" },
  ];

  // Populate chatWindow with questions
  updateChatWindow();

  // Send initial chat data after questions are initialized
  try {
    console.log("sending initial question");
    const initialResponse = await sendChatData(questions);
    // Update answers based on server response
    console.log("Initial Response received");
    initialResponse.questions.forEach((question, idx) => {
      if (questions[idx]) {
        console.log(`idx:${idx}`);
        console.log(`asked:${question.asked}`);
        console.log(`answer:${question.answer} || "n/a"`);
        questions[idx].answer = question.answer || "";
      }
    });

    // Update the chat window again with the server response
    updateChatWindow();
    console.log("Initial question answered.");
  } catch (error) {
    console.error("Error sending initial chat data:", error);
  }

  // Populate chatWindow with questions
  function updateChatWindow() {
    console.log("Updating chat window...");
    const chatWindow = document.querySelector("#chatWindow");
    chatWindow.innerHTML = ""; // Clear existing options

    questions.forEach((question) => {
      console.log(`Question: ${question.asked} Answer: ${question.answer}`);

      const messageContainer = document.createElement("div");
      messageContainer.classList.add("message-container");

      const questionDiv = document.createElement("div");
      questionDiv.classList.add("question");
      questionDiv.textContent = question.asked;

      const answerDiv = document.createElement("div");
      answerDiv.classList.add("answer");
      answerDiv.textContent = question.answer || "";

      messageContainer.appendChild(questionDiv);
      messageContainer.appendChild(answerDiv);

      chatWindow.appendChild(messageContainer);
    });

    chatWindow.scrollTop = chatWindow.scrollHeight; // Scroll to the last question
  }

  // Function to handle user input
  async function handleUserInput() {
    const chatInput = document.querySelector("#chatInput");
    const questionText = chatInput.value.trim();
    if (!questionText) return;

    questions.push({ asked: questionText, answer: "" });

    chatInput.value = "";

    updateChatWindow();

    const statusMessage = document.querySelector("#statusMessage");
    statusMessage.textContent = "Sending your question...";

    try {
      const response = await sendChatData(questions);
      response.questions.forEach((resp, idx) => {
        if (questions[idx]) {
          questions[idx].answer = resp.answer || "";
        }
      });

      updateChatWindow();
      statusMessage.textContent = "Response received!";
    } catch (error) {
      console.error("Error sending chat data:", error);
      statusMessage.textContent = "Error sending question. Please try again.";
    }
  }

  // Event listener for "Ask" button
  document.querySelector("#askButton").addEventListener("click", handleUserInput);

  // Event listener for "Enter" key in the input field
  document.querySelector("#chatInput").addEventListener("keypress", (event) => {
    if (event.key === "Enter") {
      handleUserInput();
    }
  });

  // Ensure the WebToken is available at script startup
  await getWebToken();
})();
