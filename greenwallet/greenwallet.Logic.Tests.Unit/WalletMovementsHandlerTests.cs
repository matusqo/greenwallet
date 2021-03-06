﻿using System;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;
using greenwallet.TestsCommon;
using Moq;
using NUnit.Framework;

namespace greenwallet.Logic.Tests.Unit
{
    [TestFixture]
    public class WalletMovementsHandlerTests : AutofacTestsBase
    {
        private const string ExistingWalletExternalId = "someone_existng@example.org";

        private WalletMovementsHandler _walletMovementsHandler;
        private AutoMock _autoMock;
        private Mock<IWalletTransactionRepository> _walletTransactionRepoMock;
        private Mock<IWalletRepository> _walletRepositoryMock;
        private Mock<IWalletHandler> _walletHandlerMock;

        [SetUp]
        public void Setup()
        {
            _autoMock = AutoMock.GetLoose();
            _walletTransactionRepoMock = _autoMock.Mock<IWalletTransactionRepository>();
            _walletRepositoryMock = _autoMock.Mock<IWalletRepository>();
            _walletHandlerMock = _autoMock.Mock<IWalletHandler>();
            _walletHandlerMock.Setup(handler => handler.GetWallet(ExistingWalletExternalId)).ReturnsAsync(new Wallet {ExternalId = ExistingWalletExternalId, Id = Guid.NewGuid()});
            _walletRepositoryMock.Setup(repository => repository.Get(It.IsAny<string>())).ReturnsAsync(new Wallet
            {
                ExternalId = "someone@example.org"
            });
            _walletMovementsHandler = _autoMock.Create<WalletMovementsHandler>();
        }

        [TearDown]
        public void Teardown()
        {
            _autoMock.Dispose();
            _walletTransactionRepoMock = null;
            _walletMovementsHandler = null;
        }

        [Test]
        public async Task DepositFunds_ChecksWalletExists()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            await _walletMovementsHandler.DepositFunds(walletMovementRequest);

            // Assert
            _walletHandlerMock.Verify(walletHandler => walletHandler.GetWallet(walletMovementRequest.WalletExternalId));
        }

        [Test]
        public async Task DepositFunds_ChecksTransactionExists()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();
            _walletHandlerMock.Setup(handler => handler.GetWallet(It.IsAny<string>())).ReturnsAsync(new Wallet());

            // Act
            await _walletMovementsHandler.DepositFunds(walletMovementRequest);
            
            // Assert
            _walletTransactionRepoMock.Verify(repository => repository.Get(It.IsAny<Guid>(), walletMovementRequest.MovementExternalId));
        }

        [Test]
        public async Task DepositFunds_AcceptedTransactionExists_ReturnsAccepted_DoesNotCallRepoAdd()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            _walletTransactionRepoMock.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(new WalletTransaction {Status = TransactionStatus.Accepted});
            TransactionStatus transactionStatus = await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            Assert.That(transactionStatus == TransactionStatus.Accepted);
            _walletTransactionRepoMock.Verify(repository => repository.Add(It.IsAny<WalletTransaction>()), Times.Never);
        }

        [Test]
        public async Task DepositFunds_RejectedTransactionExists_ReturnsRejected_DoesNotCallRepoAdd()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            _walletTransactionRepoMock.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(new WalletTransaction {Status = TransactionStatus.Rejected});
            TransactionStatus transactionStatus = await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            Assert.That(transactionStatus == TransactionStatus.Rejected);
            _walletTransactionRepoMock.Verify(repository => repository.Add(It.IsAny<WalletTransaction>()), Times.Never);
        }

        [Test]
        public async Task DepositFunds_CallsRepoAddWithCorrectData()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();
            _walletHandlerMock.Setup(handler => handler.GetWallet(It.IsAny<string>())).ReturnsAsync(new Wallet());

            // Act
            TransactionStatus transactionStatus = await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            Assert.That(transactionStatus == TransactionStatus.Accepted);
            _walletTransactionRepoMock.Verify(repository =>
                repository.Add(It.Is((WalletTransaction transaction) =>
                    transaction.Type == TransactionType.Deposit && transaction.Amount == 10m && transaction.ExternalId == "1234" && transaction.Status == TransactionStatus.Accepted)));
        }

        [Test]
        public void DepositFunds_MissingPlayerEmail_ThrowsException()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest_MissingPlayerEmail();

            // Act & Assert
            Assert.That(async () => await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false), Throws.Exception);
            _walletTransactionRepoMock.VerifyNoOtherCalls();
        }

        [Test]
        public void DepositFunds_MissingExternalId_ThrowsException()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest_MissingExternalId();

            // Act & Assert
            Assert.That(async () => await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false), Throws.Exception);
            _walletTransactionRepoMock.VerifyNoOtherCalls();
        }

        [Test]
        public void DepositFunds_AmountZero_ThrowsException()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest_AmountZero();

            // Act & Assert
            Assert.That(async () => await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false), Throws.Exception);
            _walletTransactionRepoMock.VerifyNoOtherCalls();
        }

        [Test]
        public void DepositFunds_AmountLtZero_ThrowsException()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest_AmountLtZero();

            // Act & Assert
            Assert.That(async () => await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false), Throws.Exception);
            _walletTransactionRepoMock.VerifyNoOtherCalls();
        }

        [Test]
        public void WithdrawFunds_MissingPlayerEmail_ThrowsException()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest_MissingPlayerEmail();

            // Act & Assert
            Assert.That(async () => await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false), Throws.Exception);
            _walletTransactionRepoMock.VerifyNoOtherCalls();
        }

        [Test]
        public void WithdrawFunds_MissingExternalId_ThrowsException()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest_MissingExternalId();

            // Act & Assert
            Assert.That(async () => await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false), Throws.Exception);
            _walletTransactionRepoMock.VerifyNoOtherCalls();
        }

        [Test]
        public void WithdrawFunds_AmountZero_ThrowsException()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest_AmountZero();

            // Act & Assert
            Assert.That(async () => await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false), Throws.Exception);
            _walletTransactionRepoMock.VerifyNoOtherCalls();
        }

        [Test]
        public void WithdrawFunds_AmountLtZero_ThrowsException()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest_AmountLtZero();

            // Act & Assert
            Assert.That(async () => await _walletMovementsHandler.DepositFunds(walletMovementRequest).ConfigureAwait(false), Throws.Exception);
            _walletTransactionRepoMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task WithdrawFunds_ChecksWalletExists()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            await _walletMovementsHandler.WithdrawFunds(walletMovementRequest);

            // Assert
            _walletHandlerMock.Verify(walletHandler => walletHandler.GetWallet(walletMovementRequest.WalletExternalId));
        }

        [Test]
        public async Task WithdrawFunds_ChecksTransactionExists()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();
            _walletHandlerMock.Setup(handler => handler.GetWallet(It.IsAny<string>())).ReturnsAsync(new Wallet());

            // Act
            await _walletMovementsHandler.WithdrawFunds(walletMovementRequest);
            
            // Assert
            _walletTransactionRepoMock.Verify(repository => repository.Get(It.IsAny<Guid>(), walletMovementRequest.MovementExternalId));
        }

        [Test]
        public async Task WithdrawFunds_CallsRepoAddWithCorrectData()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();
            _walletHandlerMock.Setup(handler => handler.GetWallet(It.IsAny<string>())).ReturnsAsync(new Wallet());

            // Act
            await _walletMovementsHandler.WithdrawFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            _walletTransactionRepoMock.Verify(repository =>
                repository.Add(It.Is((WalletTransaction transaction) =>
                    transaction.Type == TransactionType.Stake && transaction.Amount == 10m && transaction.ExternalId == "1234")));
        }

        [Test]
        public async Task WithdrawFunds_AcceptedTransactionExists_ReturnsAccepted_DoesNotCallRepoAdd()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            _walletTransactionRepoMock.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(new WalletTransaction {Status = TransactionStatus.Accepted});
            TransactionStatus transactionStatus = await _walletMovementsHandler.WithdrawFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            Assert.That(transactionStatus == TransactionStatus.Accepted);
            _walletTransactionRepoMock.Verify(repository => repository.Add(It.IsAny<WalletTransaction>()), Times.Never);
        }

        [Test]
        public async Task WithdrawFunds_RejectedTransactionExists_ReturnsRejected_DoesNotCallRepoAdd()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            _walletTransactionRepoMock.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(new WalletTransaction {Status = TransactionStatus.Rejected});
            TransactionStatus transactionStatus = await _walletMovementsHandler.WithdrawFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            Assert.That(transactionStatus == TransactionStatus.Rejected);
            _walletTransactionRepoMock.Verify(repository => repository.Add(It.IsAny<WalletTransaction>()), Times.Never);
        }

        [Test]
        public async Task WithdrawFunds_BalanceTooLow_ReturnsRejected_StoresRejected()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();
            _walletHandlerMock.Setup(walletHandler => walletHandler.GetWalletBalance(It.IsAny<Guid>())).ReturnsAsync(5m);
            _walletHandlerMock.Setup(handler => handler.GetWallet(It.IsAny<string>())).ReturnsAsync(new Wallet());

            // Act
            TransactionStatus transactionStatus = await _walletMovementsHandler.WithdrawFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            Assert.That(transactionStatus == TransactionStatus.Rejected);
            _walletTransactionRepoMock.Verify(repository => repository.Add(It.Is((WalletTransaction transaction) => transaction.Status == TransactionStatus.Rejected)));
        }

        [Test]
        public async Task WithdrawFunds_SufficientBalance_ReturnsAccepted_StoresAccepted()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();
            _walletHandlerMock.Setup(walletHandler => walletHandler.GetWalletBalance(It.IsAny<Guid>())).ReturnsAsync(10m);
            _walletHandlerMock.Setup(handler => handler.GetWallet(It.IsAny<string>())).ReturnsAsync(new Wallet());

            // Act
            TransactionStatus transactionStatus = await _walletMovementsHandler.WithdrawFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            Assert.That(transactionStatus == TransactionStatus.Accepted);
            _walletTransactionRepoMock.Verify(repository => repository.Add(It.Is((WalletTransaction transaction) => transaction.Status == TransactionStatus.Accepted)));
        }

        private static WalletMovementRequest CreateWalletMovementRequest() =>
            new WalletMovementRequest
            {
                WalletExternalId = ExistingWalletExternalId,
                Amount = 10m,
                MovementExternalId = "1234"
            };

        private static WalletMovementRequest CreateWalletMovementRequest_MissingPlayerEmail() =>
            new WalletMovementRequest
            {
                WalletExternalId = null,
                Amount = 10m,
                MovementExternalId = "1234"
            };

        private static WalletMovementRequest CreateWalletMovementRequest_MissingExternalId() =>
            new WalletMovementRequest
            {
                WalletExternalId = "someone@example.org",
                Amount = 10m,
                MovementExternalId = null
            };

        private static WalletMovementRequest CreateWalletMovementRequest_AmountZero() =>
            new WalletMovementRequest
            {
                WalletExternalId = "someone@example.org",
                Amount = 0m,
                MovementExternalId = "1234"
            };

        private static WalletMovementRequest CreateWalletMovementRequest_AmountLtZero() =>
            new WalletMovementRequest
            {
                WalletExternalId = "someone@example.org",
                Amount = -5m,
                MovementExternalId = "1234"
            };
    }
}