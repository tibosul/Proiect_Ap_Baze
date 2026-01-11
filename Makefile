SHELL := /bin/bash
.PHONY: setup run clean

setup:
	sudo bash ./setup_env.sh

run:
	@echo "Enter SQL Server Password for session:"
	@read -s password; \
	export SA_PASSWORD=$$password; \
	dotnet run --project VolunteerSystem.Avalonia

clean:
	dotnet clean
