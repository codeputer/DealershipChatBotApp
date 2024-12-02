(async function () {
  const dealerJWTToken = "{dealerJWTToken}"; // Replace with the actual dealer token
  const webTokenUrl = "{webTokenUrl}"; // URL to fetch WebChat token
  const chatEndpointUrl = "{chatEndpointUrl}"; // URL for WebChat messages

  // Function to fetch a new WebToken
  async function fetchWebToken() {
    const response = await fetch(webTokenUrl, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${dealerJWTToken}`,
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) throw new Error("Failed to fetch WebToken");

    const data = await response.json();
    const newToken = data.webToken;

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

  // Dynamically inject HTML and CSS for the chat window
  const chatContainer = document.createElement("div");
  chatContainer.id = "chatContainer";
  chatContainer.innerHTML = `
        <div id="chatWindow" contenteditable="true"></div>
        <div id="chatLog"></div>
    `;
  document.body.appendChild(chatContainer);

  // Add embedded CSS
  const style = document.createElement("style");
  style.textContent = `
        #chatContainer {
            width: 400px;
            margin: 20px auto;
            border: 1px solid #ccc;
            padding: 10px;
            font-family: Arial, sans-serif;
        }
        #chatWindow {
            width: 100%;
            height: 150px;
            border: 1px solid #ccc;
            padding: 5px;
            margin-bottom: 10px;
        }
        #chatLog {
            width: 100%;
            height: 200px;
            border: 1px solid #ccc;
            overflow-y: auto;
            padding: 5px;
            background-color: #f9f9f9;
        }
    `;
  document.head.appendChild(style);

  // Initialize questions array
  const questions = [];

  // Add event listener to capture chat messages
  document.querySelector("#chatWindow").addEventListener("input", async (e) => {
    const questionAsked = e.target.textContent;

    // Add the new question to the array
    questions.push({
      QuestionAsked: questionAsked,
      QuestionResponse: "",
    });

    console.log("Updated Questions array:", questions);

    // Send the data to the server
    const response = await sendChatData(questions);

    // Display the response in the chat log
    const chatLog = document.querySelector("#chatLog");
    const responseDiv = document.createElement("div");
    responseDiv.textContent = `Response: ${JSON.stringify(response)}`;
    chatLog.appendChild(responseDiv);

    console.log("Chat Response:", response);
  });

  // Ensure the WebToken is available at script startup
  await getWebToken();
})();
