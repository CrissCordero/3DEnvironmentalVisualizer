#acrScene for visualization?
#Maybe for mapping visualization: https://www.generic-mapping-tools.org/about/
#PyVista for visualization?

#WOA DOCUMENTATION----------------------------
#   TEMPERATURE: https://repository.library.noaa.gov/view/noaa/60599
#   SALINITY: https://repository.library.noaa.gov/view/noaa/60600
#------------------------------------------------


#WOA DATASET------------------------------------
#COL 0: Latitude (degrees North)
#COL 1: Longitude (degrees East)
#COL 2+: Temperature values ordered by increasing Depth (meters)
    #WOA has 0-2000 meters, marked 'NaN' if depth is bellow seafloor or land
#------------------------------------------------


import requests
from bs4 import BeautifulSoup
import pandas as pd


#OPEN LINK -> RETRIEVE DESIRED FILES

#base_url = "https://www.ncei.noaa.gov/data/oceans/woa/"
base_url = "https://www.ncei.noaa.gov/data/oceans/woa/WOA23/DATA/temperature/csv/95A4/1.00/"
#Note need to add better system for getting urls at some point


def retrieve_files(file):

    res = requests.get(base_url)
    status = res.status_code #200=success
    # Check if the request was successful
    if status != 200: print(f"Failed to retrieve page. Status code: {status}")

    rawhtmlcontent = res.content
    soup = BeautifulSoup(rawhtmlcontent, 'html.parser')

    # Find all anchor (<a>) tags on the page
    links = soup.find_all('a')
    if not links:
        #print("No links found on the page.")
        return False

    for link in links:
        link_text = link.get_text(strip=True) # Extracts the visible text between <a> and </a>
        if link_text == file:
            #print(f"Found the file: {link_text}")
            href = link.get('href')
            link = f"{base_url}{href}"  # Construct the full URL to the file
            # Pandas automatically detects the .gz extension and streams it
            #print(pd.read_csv(link,nrows=1))
            NOAA_data=pd.read_csv(link,header=1)
            NOAA_data=NOAA_data.rename(columns={'#COMMA SEPARATED LATITUDE':'latitude',' LONGITUDE':'longitude',' AND VALUES AT DEPTHS (M):0':'0'})
            #NOAA_data=NOAA_data.set_index(['latitude','longitude'])#maybe could set index to lat/long? Not sure how helpful it would be though
            return NOAA_data  #Return files to called function


def NARDDAP_remote_access():#incomplete function do not use
    link1="https://cwcgom.aoml.noaa.gov/erddap/griddap/WOA_TEMP_d48d_916b_3478.csv?t_an%5B(1988-12-16T00:00:00Z):1:(1988-12-16T00:00:00Z)%5D%5B(95):1:(450.0)%5D%5B(-15.875):1:(89.875)%5D%5B(12.125):1:(179.875)%5D"
    link2="https://cwcgom.aoml.noaa.gov/erddap/griddap/miamicurrents.csv?u_current%5B(2026-10-20):1:(2026-10-20T00:00:00Z)%5D%5B(-64.7):1:(64.7)%5D%5B(-179.9):1:(179.9)%5D,v_current%5B(2026-10-20):1:(2026-10-20T00:00:00Z)%5D%5B(-64.7):1:(64.7)%5D%5B(-179.9):1:(179.9)%5D"
    data = pd.read_csv(link1,skiprows=[1])
    data=data.pivot(index=["latitude","longitude"], columns=["depth"],values="t_an")
    data=data.reset_index()
    return data


def ERDDAP_read_CSV(CSV_name):#get data from ERDDAP CSV located in same folder
    data = pd.read_csv(CSV_name,skiprows=[1])
    data=data.pivot(index=["latitude","longitude"], columns=["depth"],values="t_an")#restructure dataframe to keep consistent with WOA dataframes
    data=data.reset_index()
    return data
     

def data_set_geographic_range(NOAA_data,lat_min,lat_max,long_min,long_max):#removes data outside given latitude/longitude range
    if(lat_min>lat_max):#check for errors
            print("error given latitude depth is less than the minimum: Returning data as is")
            return NOAA_data
    if(long_min>long_max):#check for errors
            print("error given max longitude is less than the minimum: Returning data as is")
            return NOAA_data
    NOAA_data=NOAA_data[NOAA_data['latitude']>=lat_min]#only include data within latitude/longitude range
    NOAA_data=NOAA_data[NOAA_data['latitude']<=lat_max]#only include data within latitude/longitude range
    NOAA_data=NOAA_data[NOAA_data['longitude']>=long_min]#only include data within latitude/longitude range
    NOAA_data=NOAA_data[NOAA_data['longitude']<=long_max]#only include data within latitude/longitude range
    return NOAA_data #return back to user


def data_set_depth_range(NOAA_data,depth_min,depth_max):#removes data outside given depth range
    if(depth_min>depth_max):#check for errors
        print("error given max depth is less than the minimum: Returning data as is")
        return NOAA_data
    for column in NOAA_data.columns[2:]:#search every column after lat/long columns
        if(int(column)<depth_min or int(column)>depth_max):
        
            NOAA_data=NOAA_data.drop(columns=[column])
    return NOAA_data #return back to user


def data_retrieve_and_set_all_range(file,lat_min,lat_max,long_min,long_max,depth_min,depth_max):#Does file/lat/long/depth functions in 1
    data=retrieve_files(file)  # Pass the full URL to the retrieve_files function
    data=data_set_geographic_range(data,lat_min,lat_max,long_min,long_max)#
    data=data_set_depth_range(data, depth_min, depth_max)
    return data

def data_change_scope(NOAA_data,x_latitude,x_longitude): #Only includes every x number of data points
    lat_unique=NOAA_data['latitude'].nunique()-1 #num of unique lat vals
    long_unique=NOAA_data['longitude'].nunique()-1 #num of unique long vals
    lat_uniquer=NOAA_data['latitude'].unique() #list of unique lat vals
    long_uniquer=NOAA_data['longitude'].unique() # list of unique long vals
    for x in range(lat_unique):
        if(x % x_latitude !=0):#if remainder is not 0 drop data point
            NOAA_data=NOAA_data[NOAA_data['latitude']!=lat_uniquer[x]]
    for x in range(long_unique):
            if(x % x_longitude !=0):#if remainder is not 0 drop data point
                NOAA_data=NOAA_data[NOAA_data['longitude']!=long_uniquer[x]]
    return NOAA_data


def main():#only here for testing purposes
    file= 'woa23_95A4_t00an01.csv.gz'
    # Call the function to build the URL and retrieve files
    #data=retrieve_files(file)  # Pass the full URL to the retrieve_files function
    data=ERDDAP_read_CSV("3DEnvironmentalVisualizer-main/3DEnvironmentalVisualizer-main/WOA_TEMP_d48d_916b_3478_baca_5138_ecc5.csv")
    data=data_set_geographic_range(data,-19,10,10,100)#
    data=data_set_depth_range(data, 0, 500)
    data=data_change_scope(data,10,10)
    #data2=data_retrieve_and_set_all_range(file,-19,100,-90,50,10, 400)
    print(data)
    #print(data2)
#main()
