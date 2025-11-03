from pyngrok import ngrok
import time

print("Starting ngrok tunnel for ERP application...")
print("Port 80 -> Public URL")

try:
    # Create tunnel to port 80
    public_tunnel = ngrok.connect(80)
    
    print("\n" + "="*50)
    print("✅ SUCCESS! Your ERP application is now PUBLIC!")
    print("="*50)
    print(f"🌐 Public URL: {public_tunnel.public_url}")
    print(f"🚀 ERP Access: {public_tunnel.public_url}/NewERP/")
    print("📝 Login: sapl0002 / Sapl@0002")
    print("="*50)
    print("\nTunnel is running... Press Ctrl+C to stop")
    
    # Keep tunnel alive
    try:
        while True:
            time.sleep(30)
            print(f"✅ Tunnel active: {public_tunnel.public_url}/NewERP/")
    except KeyboardInterrupt:
        print("\n🛑 Tunnel stopped by user")
        
except Exception as e:
    print(f"❌ Error creating tunnel: {e}")
    print("\nPlease set up ngrok authentication:")
    print("1. Go to: https://dashboard.ngrok.com/signup")
    print("2. Get your authtoken from: https://dashboard.ngrok.com/get-started/your-authtoken")
    print("3. Run: ngrok config add-authtoken YOUR_TOKEN")
