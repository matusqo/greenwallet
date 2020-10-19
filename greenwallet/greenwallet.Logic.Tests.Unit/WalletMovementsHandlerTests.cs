using System;
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
        private WalletMovementsHandler _walletMovementsHandler;
        private AutoMock _autoMock;
        private Mock<IWalletTransactionRepository> _walletTransactionRepoMock;
        private Mock<IWalletRepository> _walletRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _autoMock = AutoMock.GetLoose();
            _walletTransactionRepoMock = _autoMock.Mock<IWalletTransactionRepository>();
            _walletRepositoryMock = _autoMock.Mock<IWalletRepository>();
            _walletRepositoryMock.Setup(repository => repository.Get(It.IsAny<string>())).ReturnsAsync(new Wallet
            {
                PlayerEmail = "someone@example.org",
                Balance = 100m
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
            _walletRepositoryMock.Verify(repository => repository.Get(walletMovementRequest.PlayerEmail));
        }

        [Test]
        public async Task DepositFunds_ChecksTransactionExists()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            await _walletMovementsHandler.DepositFunds(walletMovementRequest);
            
            // Assert
            _walletTransactionRepoMock.Verify(repository => repository.Get(walletMovementRequest.ExternalId));
        }

        [Test]
        public async Task DepositFunds_AcceptedTransactionExists_ReturnsAccepted_DoesNotCallRepoAdd()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            _walletTransactionRepoMock.Setup(repository => repository.Get(It.IsAny<string>())).ReturnsAsync(new WalletTransaction {Status = TransactionStatus.Accepted});
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
            _walletTransactionRepoMock.Setup(repository => repository.Get(It.IsAny<string>())).ReturnsAsync(new WalletTransaction {Status = TransactionStatus.Rejected});
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
            _walletRepositoryMock.Verify(repository => repository.Get(walletMovementRequest.PlayerEmail));
        }

        [Test]
        public async Task WithdrawFunds_ChecksTransactionExists()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            await _walletMovementsHandler.WithdrawFunds(walletMovementRequest);
            
            // Assert
            _walletTransactionRepoMock.Verify(repository => repository.Get(walletMovementRequest.ExternalId));
        }

        [Test]
        public async Task WithdrawFunds_AcceptedTransactionExists_ReturnsAccepted_DoesNotCallRepoAdd()
        {
            // Arrange
            WalletMovementRequest walletMovementRequest = CreateWalletMovementRequest();

            // Act
            _walletTransactionRepoMock.Setup(repository => repository.Get(It.IsAny<string>())).ReturnsAsync(new WalletTransaction {Status = TransactionStatus.Accepted});
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
            _walletTransactionRepoMock.Setup(repository => repository.Get(It.IsAny<string>())).ReturnsAsync(new WalletTransaction {Status = TransactionStatus.Rejected});
            TransactionStatus transactionStatus = await _walletMovementsHandler.WithdrawFunds(walletMovementRequest).ConfigureAwait(false);

            // Assert
            Assert.That(transactionStatus == TransactionStatus.Rejected);
            _walletTransactionRepoMock.Verify(repository => repository.Add(It.IsAny<WalletTransaction>()), Times.Never);
        }

        private static WalletMovementRequest CreateWalletMovementRequest() =>
            new WalletMovementRequest
            {
                PlayerEmail = "someone@example.org",
                Amount = 10m,
                ExternalId = "1234"
            };

        private static WalletMovementRequest CreateWalletMovementRequest_MissingPlayerEmail() =>
            new WalletMovementRequest
            {
                PlayerEmail = null,
                Amount = 10m,
                ExternalId = "1234"
            };

        private static WalletMovementRequest CreateWalletMovementRequest_MissingExternalId() =>
            new WalletMovementRequest
            {
                PlayerEmail = "someone@example.org",
                Amount = 10m,
                ExternalId = null
            };

        private static WalletMovementRequest CreateWalletMovementRequest_AmountZero() =>
            new WalletMovementRequest
            {
                PlayerEmail = "someone@example.org",
                Amount = 0m,
                ExternalId = "1234"
            };

        private static WalletMovementRequest CreateWalletMovementRequest_AmountLtZero() =>
            new WalletMovementRequest
            {
                PlayerEmail = "someone@example.org",
                Amount = -5m,
                ExternalId = "1234"
            };
    }
}