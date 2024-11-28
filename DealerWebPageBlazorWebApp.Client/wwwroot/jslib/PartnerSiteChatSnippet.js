(function () {
  const dealershipId = 'your-dealer-id'; // Replace with actual dealerId
  const tokenType = 'WebChatToken'; // Replace with actual token type
  const apiUrl = 'https://your-api-url/api/token'; // Replace with actual API URL

  async function getJwtToken() {
    const response = await fetch(apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ dealershipId, tokenType })
    });

    if (response.ok) {
      const token = await response.text();
      openChatWindow(token);
    } else {
      console.error('Failed to retrieve JWT token');
    }
  }

  function openChatWindow(token) {
    const chatWindowUrl = `https://your-chat-url/PartnerChatWindow/${token}`; // Replace with actual chat window URL
    window.open(chatWindowUrl, '_blank', 'width=400,height=600');
  }

  // Call the function to get the JWT token and open the chat window
  getJwtToken();
})();