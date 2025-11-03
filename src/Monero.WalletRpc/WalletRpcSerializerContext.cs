using System.Text.Json.Serialization;

using RpcRequestOfCreateAddressParameters = Monero.WalletRpc.RpcRequest<Monero.WalletRpc.CreateAddress.CreateAddressParameters>;
using RpcResponseOfCreateAddressResult = Monero.WalletRpc.RpcResponse<Monero.WalletRpc.CreateAddress.CreateAddressResult>;

using RpcRequestOfGetAddressParameters = Monero.WalletRpc.RpcRequest<Monero.WalletRpc.GetAddress.GetAddressParameters>;
using RpcResponseOfGetAddressResult = Monero.WalletRpc.RpcResponse<Monero.WalletRpc.GetAddress.GetAddressResult>;
using GetAddressAddressEntry = Monero.WalletRpc.GetAddress.AddressEnty;

using RpcRequestOfGetAddressIndexParameters = Monero.WalletRpc.RpcRequest<Monero.WalletRpc.GetAddressIndex.GetAddressIndexParameters>;
using RpcResponseOfGetAddressIndexResult = Monero.WalletRpc.RpcResponse<Monero.WalletRpc.GetAddressIndex.GetAddressIndexResult>;
using GetAddressIndexAddressIndex = Monero.WalletRpc.GetAddressIndex.AddressIndex;

using RpcRequestOfGetTransfersParameters = Monero.WalletRpc.RpcRequest<Monero.WalletRpc.GetTransfers.GetTransfersParameters>;
using RpcResponseOfGetTransfersResult = Monero.WalletRpc.RpcResponse<Monero.WalletRpc.GetTransfers.GetTransfersResult>;
using GetTransfersTransfer = Monero.WalletRpc.GetTransfers.Transfer;
using GetTransfersDestination = Monero.WalletRpc.GetTransfers.Destination;
using GetTransfersAddressIndex = Monero.WalletRpc.GetTransfers.AddressIndex;

using RpcRequestOfSignParameters = Monero.WalletRpc.RpcRequest<Monero.WalletRpc.Sign.SignParameters>;
using RpcResponseOfSignResult = Monero.WalletRpc.RpcResponse<Monero.WalletRpc.Sign.SignResult>;

using RpcRequestOfVerifyParameters = Monero.WalletRpc.RpcRequest<Monero.WalletRpc.Verify.VerifyParameters>;
using RpcResponseOfVerifyResult = Monero.WalletRpc.RpcResponse<Monero.WalletRpc.Verify.VerifyResult>;

namespace Monero.WalletRpc;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(RpcRequestOfCreateAddressParameters), TypeInfoPropertyName = nameof(RpcRequestOfCreateAddressParameters))]
[JsonSerializable(typeof(RpcResponseOfCreateAddressResult), TypeInfoPropertyName = nameof(RpcResponseOfCreateAddressResult))]
[JsonSerializable(typeof(RpcRequestOfGetAddressParameters), TypeInfoPropertyName = nameof(RpcRequestOfGetAddressParameters))]
[JsonSerializable(typeof(RpcResponseOfGetAddressResult), TypeInfoPropertyName = nameof(RpcResponseOfGetAddressResult))]
[JsonSerializable(typeof(GetAddressAddressEntry), TypeInfoPropertyName = nameof(GetAddressAddressEntry))]
[JsonSerializable(typeof(RpcRequestOfGetAddressIndexParameters), TypeInfoPropertyName = nameof(RpcRequestOfGetAddressIndexParameters))]
[JsonSerializable(typeof(RpcResponseOfGetAddressIndexResult), TypeInfoPropertyName = nameof(RpcResponseOfGetAddressIndexResult))]
[JsonSerializable(typeof(GetAddressIndexAddressIndex), TypeInfoPropertyName = nameof(GetAddressIndexAddressIndex))]
[JsonSerializable(typeof(RpcRequestOfGetTransfersParameters), TypeInfoPropertyName = nameof(RpcRequestOfGetTransfersParameters))]
[JsonSerializable(typeof(RpcResponseOfGetTransfersResult), TypeInfoPropertyName = nameof(RpcResponseOfGetTransfersResult))]
[JsonSerializable(typeof(RpcRequestOfSignParameters), TypeInfoPropertyName = nameof(RpcRequestOfSignParameters))]
[JsonSerializable(typeof(RpcResponseOfSignResult), TypeInfoPropertyName = nameof(RpcResponseOfSignResult))]
[JsonSerializable(typeof(RpcRequestOfVerifyParameters), TypeInfoPropertyName = nameof(RpcRequestOfVerifyParameters))]
[JsonSerializable(typeof(RpcResponseOfVerifyResult), TypeInfoPropertyName = nameof(RpcResponseOfVerifyResult))]
[JsonSerializable(typeof(GetTransfersTransfer), TypeInfoPropertyName = nameof(GetTransfersTransfer))]
[JsonSerializable(typeof(GetTransfersDestination), TypeInfoPropertyName = nameof(GetTransfersDestination))]
[JsonSerializable(typeof(GetTransfersAddressIndex), TypeInfoPropertyName = nameof(GetTransfersAddressIndex))]
internal partial class WalletRpcSerializerContext : JsonSerializerContext;
