// Copyright (c) 2014 George Kimionis
// Distributed under the GPLv3 software license, see the accompanying file LICENSE or http://opensource.org/licenses/GPL-3.0

// Ported to Namecoin by Derrick Slopey Feb 25, 2014

namespace NamecoinLib.RPC
{
    //  Note: Do not alter the capitalization of the enum members as they are being cast as-is to the RPC server
    public enum RpcMethods
    {
        // -= NMC Specific =-
        
        name_show,

        // -= End NMC Specific =-

        addmultisigaddress,
        addnode,
        backupwallet,
        createmultisig,
        createrawtransaction,
        decoderawtransaction,
        dumpprivkey,
        encryptwallet,
        getaccount,
        getaccountaddress,
        getaddednodeinfo,
        getaddressesbyaccount,
        getbalance,
        getbestblockhash,
        getblock,
        getblockcount,
        getblockhash,
        getblocktemplate,
        getconnectioncount,
        getdifficulty,
        getgenerate,
        gethashespersec,
        getinfo,
        getmininginfo,
        getnewaddress,
        getpeerinfo,
        getrawchangeaddress,
        getrawmempool,
        getrawtransaction,
        getreceivedbyaccount,
        getreceivedbyaddress,
        gettransaction,
        gettxout,
        gettxoutsetinfo,
        getwork,
        help,
        importprivkey,
        keypoolrefill,
        listaccounts,
        listaddressgroupings,
        listlockunspent,
        listreceivedbyaccount,
        listreceivedbyaddress,
        listsinceblock,
        listtransactions,
        listunspent,
        lockunspent,
        move,
        sendfrom,
        sendmany,
        sendrawtransaction,
        sendtoaddress,
        setaccount,
        setgenerate,
        settxfee,
        signmessage,
        signrawtransaction,
        stop,
        submitblock,
        validateaddress,
        verifymessage,
        walletlock,
        walletpassphrase,
        walletpassphrasechange
    }
}