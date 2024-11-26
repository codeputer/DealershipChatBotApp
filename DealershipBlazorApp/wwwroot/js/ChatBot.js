class ChatBot {
  constructor(options) {
    this.options = options;
    this.chatWindow = null;
  }

  init() {
    this.createChatWindow();
    if (this.options.onInit) {
      this.options.onInit();
    }
  }

  createChatWindow() {
    this.chatWindow = document.createElement('div');
    this.chatWindow.className = 'chat-window';
    this.chatWindow.innerHTML = '<p>ChatBot is ready to assist you!</p>';
    document.body.appendChild(this.chatWindow);
  }

  start() {
    this.chatWindow.style.display = 'block';
  }

  stop() {
    this.chatWindow.style.display = 'none';
  }
}

// Initialize the ChatBot when the document is ready
document.addEventListener('DOMContentLoaded', function () {
  var token = 'PRECONFIGURED_TOKEN'; // This token should be securely generated and provided

  var bot = new ChatBot({
    onInit: function () {
      fetch('/api/messages', {
        headers: {
          'Authorization': 'Bearer ' + token
        }
      })
        .then(response => {
          if (response.status === 200) {
            bot.start();
          } else {
            console.error('Authentication failed');
          }
        });
    }
  });
  bot.init();
});
