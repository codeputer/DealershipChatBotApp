window.BlazorWebChat = {
  renderWebChat: function (config) {
    const styleOptions = config.styleOptions;
    const store = window.WebChat.createStore(config.store, ({ dispatch }) => next => action => {
      if (action.type === 'DIRECT_LINE/CONNECT_FULFILLED') {
        dispatch({
          type: 'WEB_CHAT/SEND_EVENT',
          payload: {
            name: 'webchat/join',
            value: { language: window.navigator.language }
          }
        });
      }
      return next(action);
    });

    window.WebChat.renderWebChat(
      {
        directLine: window.WebChat.createDirectLine({
          token: config.directLine.token,
          domain: config.directLine.domain
        }),
        store,
        styleOptions
      },
      document.getElementById('webchat')
    );
  }
};