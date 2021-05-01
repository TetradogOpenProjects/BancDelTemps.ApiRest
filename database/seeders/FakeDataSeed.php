<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;

class FakeDataSeed extends Seeder
{
    /**
     * Run the database seeds.
     *
     * @return void
     */
    public function run()
    {
        $categories = Category::factory(15)->create();
        $users=User::factory(25)->create();
        $locations=Location::factory(30)->create();
        $files=File::factory(25)->create();
        $userDocument=UserDocument::factory(25)->create();
        $userPermission=UserPermission::factory(25)->create();
        $userRequest=UserRequest::factory(15)->create();
        $userMessager=Message::factory(150)->create();
        

    


    }
}
