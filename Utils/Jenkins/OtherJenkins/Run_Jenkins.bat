@echo off
java -Dhudson.security.csrf.GlobalCrumbIssuerConfiguration.DISABLE_CSRF_PROTECTION=true -jar jenkins.war --enable-future-java
pause