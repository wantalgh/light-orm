light-orm
=========
This is a light fast and fit ORM tool for accessing relational database.
I am sorry for only writing Chinese comments in code and haven't written any documents for you.

=========
此工具是对ADO.NET的简单ORM封装，可以简化访问数据库的动作。
相比Hibernate和Entity Framework等重型ORM工具，此工具小轻快，更适合互联网项目开发。

使用步骤：
一、在应用程序Config文件里配数据库连接串。
二、使用连接串的配置名作参数建立SqlDataSource的实例。
三、调用SqlDataSource实例的方法。方法的具体用法可参考DbExtensionTest项目里的调用说明。
