﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using LibraryManagementSystem.Publishing.Contexts;
using LibraryManagementSystem.Publishing.Repositories;
using LibraryManagementSystem.Publishing.Services;
using LibraryManagementSystem.Publishing.UniteOfWorks;

namespace LibraryManagementSystem.Publishing
{
    public class PublishingModule : Module
    {
        private readonly string _connectionString;
        private readonly string _migrationAssemblyName;


        public PublishingModule(string connectionString, string migrationAssemblyName)
        {
            _connectionString = connectionString;
            _migrationAssemblyName = migrationAssemblyName;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PublishingDbContext>().AsSelf()
                .WithParameter("connectionString", _connectionString)
                .WithParameter("migrationAssemblyName", _migrationAssemblyName)
                .InstancePerLifetimeScope();

            builder.RegisterType<PublishingDbContext>().As<IPublishingDbContext>()
                .WithParameter("connectionString", _connectionString)
                .WithParameter("migrationAssemblyName", _migrationAssemblyName)
                .InstancePerLifetimeScope();

            builder.RegisterType<AuthorRepository>().As<IAuthorRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<BookRepository>().As<IBookRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<AuthorService>().As<IAuthorService>()
                .InstancePerLifetimeScope();
            builder.RegisterType<BookService>().As<IBookService>()
                .InstancePerLifetimeScope();
            builder.RegisterType<PublishingUniteOfWork>().As<IPublishingUniteOfWork>()
                .InstancePerLifetimeScope();


            base.Load(builder);
        }
    }
}
