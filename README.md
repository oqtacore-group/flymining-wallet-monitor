# FlyMining Wallet Monitor

A Windows desktop tool that watches a Bitcoin mining operation from both ends: the **hardware**
(ASIC miners on the local network) and the **money** (on-chain wallet balances and exchange
activity). It shares its miner-monitoring core with [FlyMining Monitor](../flymining-monitor) and
adds wallet and exchange tracking on top. Published as a reference for how we operate SHA-256
mining hardware and reconcile its output.

## What it does

### Monitors the mining hardware

Like FlyMining Monitor, it speaks the **CGMiner / bmminer-style JSON API** exposed by
Antminer-class hardware — issuing `summary`, `stats` and `pools` to read hashrate, temperatures,
fan speeds and uptime, and the `addpool` / `enablepool` / `removepool` control commands to manage
Stratum pool configuration across the fleet.

### Tracks wallet balances and exchange orders

On top of the hardware view it follows the treasury side:

- **Bitcoin balances** for the operation's wallets;
- **exchange order history** via the Bittrex API (`BittrexOrdersForm`, `Wallets.parseOrderHistory`),
  so mined coin that was moved to an exchange and sold could be reconciled against payouts.

### Reports to the backend

Status and alerts are pushed to the FlyMining / FlySecure reporting API, and the tool can email
operators when a miner drops off or a threshold is crossed.

## Repository layout

C# WinForms. Notable files:

| File | Role |
|---|---|
| `MainWindow.cs` | Fleet polling loop and pool control. |
| `BTCBalanceForm.cs`, `BTCSettingsForm.cs` | Bitcoin balance tracking. |
| `BittrexOrdersForm.cs` | Exchange order-history parsing and reconciliation. |
| `Wallets.cs` | Wallet and exchange-request helpers. |
| `Log.cs` | Reporting/alerting to the backend API. |

## Technology

C# / .NET, WinForms, the CGMiner/bmminer JSON API, the Bittrex exchange API.

## Status

Archived snapshot, kept as a reference. Not maintained. The default `root` miner login it uses is
the vendor default for this hardware, not a secret; the former reporting-mailbox credential that
once appeared in a commented-out line has been removed.
