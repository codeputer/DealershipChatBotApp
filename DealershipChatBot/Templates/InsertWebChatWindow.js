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
        /* Style for the chat window */
        #chatWindow {
          width: 100%;
          height: 300px;
          overflow-y: auto;
          border: 1px solid #ccc;
          padding: 10px;
          font-family: Arial, sans-serif;
          font-size: 14px;
        }

        /* Container for each message */
        .message-container {
          margin-bottom: 10px;
        }

        /* Style for the question */
        .question {
          text-align: left;
          font-weight: bold;
          margin-bottom: 5px;
        }

        /* Style for the answer */
        .answer {
          text-align: right;
          word-wrap: break-word;
          max-width: 50%; /* Adjust this value as needed */
          margin-left: auto; /* Push the answer to the right */
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
    console.log("sending initial question")
    const initialResponse = await sendChatData(questions);
    // Update answers based on server response
    console.log("Initial Response received")
    initialResponse.questions.forEach((question, idx) => {
      if (questions[idx]) {
        console.log(`idx:${idx}`)
        console.log(`asked:${question.asked}`)
        console.log(`answer:${question.answer} || "n/a"`)
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

      // Create a container for question and answer
      const messageContainer = document.createElement("div");
      messageContainer.classList.add("message-container");

      // Create a div for the question
      const questionDiv = document.createElement("div");
      questionDiv.classList.add("question");
      questionDiv.textContent = question.asked;

      // Create a div for the answer
      const answerDiv = document.createElement("div");
      answerDiv.classList.add("answer");
      answerDiv.textContent = question.answer || "";

      // Append question and answer to the container
      messageContainer.appendChild(questionDiv);
      messageContainer.appendChild(answerDiv);

      // Append the container to the chat window
      chatWindow.appendChild(messageContainer);
    });

    chatWindow.scrollTop = chatWindow.scrollHeight; // Scroll to the last question
  }


  // Add event listener for the "Ask" button
  document.querySelector("#askButton").addEventListener("click", async () => {
    console.log("Ask button clicked!");
    const chatInput = document.querySelector("#chatInput");
    const questionText = chatInput.value.trim();
    if (!questionText) return;

    // Add the new question to the array
    questions.push({ asked: questionText, answer: "" });

    // Clear the input box
    chatInput.value = "";

    // Update the chat window with the new question
    updateChatWindow();

    // Display status message
    const statusMessage = document.querySelector("#statusMessage");
    statusMessage.textContent = "Sending your question...";
    console.log(statusMessage.textContent);

    // Send the data to the server
    try {
      const response = await sendChatData(questions);
      // Update answers based on server response
      response.questions.forEach((resp, idx) => {
        if (questions[idx]) {
          questions[idx].answer = resp.answer || "";
        }
      });

      // Update the chat window again with the server response
      updateChatWindow();

      // Clear status message
      statusMessage.textContent = "Response received!";
    } catch (error) {
      console.error("Error sending chat data:", error);
      statusMessage.textContent = "Error sending question. Please try again.";
    }
  });

  // Initialize the chat window with the default question
  updateChatWindow();

  // Ensure the WebToken is available at script startup
  await getWebToken();
})();
