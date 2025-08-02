# Giai đoạn 1: Build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Định nghĩa một đối số để chỉ định thư mục chứa mã nguồn.
# Giá trị này sẽ được truyền từ Jenkinsfile.
ARG BUILD_CONTEXT

# Sao chép các file project và restore package
# Lệnh COPY này sẽ sử dụng BUILD_CONTEXT để tìm file .csproj
COPY ["${BUILD_CONTEXT}/WebBanHangOnline.csproj", "./"]
RUN dotnet restore "WebBanHangOnline.csproj"

# Sao chép toàn bộ mã nguồn
# Lệnh COPY này sẽ sao chép toàn bộ nội dung từ thư mục BUILD_CONTEXT
COPY "${BUILD_CONTEXT}/." .
RUN dotnet build "WebBanHangOnline.csproj" -c Release -o /app/build

# Publish ứng dụng
FROM build AS publish
RUN dotnet publish "WebBanHangOnline.csproj" -c Release -o /app/publish

# Giai đoạn 2: Tạo image để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebBanHangOnline.dll"]
