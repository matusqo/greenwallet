using System;
using Autofac.Extras.Moq;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;
using greenwallet.TestsCommon;
using Moq;
using NUnit.Framework;

namespace greenwallet.Logic.Tests.Unit
{
    [TestFixture]
    public class WalletHandlerTests : AutofacTestsBase
    {
        private AutoMock _autoMock;
        private Mock<IWalletTransactionRepository> _walletTransactionRepoMock;
        private Mock<IWalletRepository> _walletRepositoryMock;
        private WalletHandler _walletHandler;
        private string _existingWalletExternalId;

        [SetUp]
        public void Setup()
        {
            _autoMock = AutoMock.GetLoose();
            _walletTransactionRepoMock = _autoMock.Mock<IWalletTransactionRepository>();
            _walletRepositoryMock = _autoMock.Mock<IWalletRepository>();
            _existingWalletExternalId = "someone_existing@example.org";
            _walletRepositoryMock.Setup(repository => repository.Get(_existingWalletExternalId)).ReturnsAsync(new Wallet {ExternalId = _existingWalletExternalId, Id = Guid.NewGuid()});
            _walletHandler = _autoMock.Create<WalletHandler>();
        }

        [TearDown]
        public void Teardown()
        {
            _autoMock.Dispose();
            _walletTransactionRepoMock = null;
            _walletHandler = null;
        }

        [Test]
        public void RegisterNew_MissingExternalId_Throws()
        {
            Assert.That(async () => await _walletHandler.RegisterNew(null).ConfigureAwait(false), Throws.Exception);
            Assert.That(async () => await _walletHandler.RegisterNew("").ConfigureAwait(false), Throws.Exception);
        }

        [Test]
        public void RegisterNew_WalletExists_Throws()
        {
            // Act & Assert
            Assert.That(async () => await _walletHandler.RegisterNew(_existingWalletExternalId).ConfigureAwait(false), Throws.Exception);
        }

        [Test]
        public void RegisterNew_WalletDoesNotExist_StoresNew()
        {
            // Act & Assert
            var notExistingId = "someone_not_existing@example.org";
            Assert.That(async () => await _walletHandler.RegisterNew(notExistingId).ConfigureAwait(false), Throws.Nothing);
            _walletRepositoryMock.Verify(repository => repository.Add(It.Is((Wallet wallet) => wallet.ExternalId == notExistingId)));
        }
    }
}