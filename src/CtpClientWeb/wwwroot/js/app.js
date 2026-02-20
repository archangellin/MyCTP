const state = {
  accounts: new Map(),
  currentAccountId: null,
};

const accountSelector = document.getElementById('accountSelector');
const availableFunds = document.getElementById('availableFunds');
const balance = document.getElementById('balance');

const connection = new signalR.HubConnectionBuilder().withUrl('/hubs/trading').withAutomaticReconnect().build();

function ensureAccount(accountId) {
  if (!state.accounts.has(accountId)) {
    state.accounts.set(accountId, { funds: null, positions: [], orders: [], trades: [], ticks: [] });
  }
  return state.accounts.get(accountId);
}

function render() {
  if (!state.currentAccountId) return;
  const data = ensureAccount(state.currentAccountId);

  availableFunds.textContent = data.funds?.availableFunds?.toFixed(2) ?? '0.00';
  balance.textContent = data.funds?.balance?.toFixed(2) ?? '0.00';

  document.getElementById('positionsBody').innerHTML = data.positions.map(p =>
    `<tr><td>${p.instrumentId}</td><td>${p.longQty}</td><td>${p.shortQty}</td><td>${p.netQty}</td><td>${Number(p.avgPrice).toFixed(2)}</td></tr>`
  ).join('');

  document.getElementById('orders').innerHTML = data.orders.map(x => `<li>${x.time} ${x.instrumentId} ${x.status} ${x.volumeTraded}/${x.volumeTotalOriginal}</li>`).join('');
  document.getElementById('trades').innerHTML = data.trades.map(x => `<li>${x.time} ${x.instrumentId} ${x.direction} ${x.volume}@${x.price}</li>`).join('');
  document.getElementById('ticks').innerHTML = data.ticks.map(x => `<li>${x.time} ${x.instrumentId} Last:${x.lastPrice} Bid:${x.bidPrice1} Ask:${x.askPrice1}</li>`).join('');
}

function prependAndLimit(list, item, max = 50) {
  list.unshift(item);
  if (list.length > max) list.length = max;
}

connection.on('FundsReceived', funds => {
  const data = ensureAccount(funds.accountId);
  data.funds = funds;
  render();
});

connection.on('OrderReceived', order => {
  const data = ensureAccount(order.accountId);
  prependAndLimit(data.orders, order);
  render();
});

connection.on('TradeReceived', trade => {
  const data = ensureAccount(trade.accountId);
  prependAndLimit(data.trades, trade);
  render();
});

connection.on('PositionsUpdated', positions => {
  if (!positions.length) return;
  const accountId = positions[0].accountId;
  const data = ensureAccount(accountId);
  data.positions = positions;
  render();
});

connection.on('TickReceived', tick => {
  const data = ensureAccount(tick.accountId);
  prependAndLimit(data.ticks, tick);
  render();
});

accountSelector.addEventListener('change', async e => {
  const accountId = e.target.value;
  if (state.currentAccountId) {
    await connection.invoke('LeaveAccount', state.currentAccountId);
  }
  state.currentAccountId = accountId;
  await connection.invoke('JoinAccount', accountId);
  render();
});

document.getElementById('loginBtn').addEventListener('click', async () => {
  const payload = {
    brokerId: document.getElementById('brokerId').value,
    userId: document.getElementById('userId').value,
    password: document.getElementById('password').value,
    investorId: document.getElementById('investorId').value,
    symbols: document.getElementById('symbols').value.split(',').map(x => x.trim()).filter(Boolean),
  };

  const response = await fetch('/api/accounts/login', {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(payload),
  });

  const account = await response.json();
  ensureAccount(account.accountId);

  if (![...accountSelector.options].find(x => x.value === account.accountId)) {
    const option = document.createElement('option');
    option.textContent = account.accountId;
    option.value = account.accountId;
    accountSelector.appendChild(option);
  }

  accountSelector.value = account.accountId;
  accountSelector.dispatchEvent(new Event('change'));
});

(async () => {
  await connection.start();
})();
