<?php

namespace Database\Factories;

use App\Models\UserRequest;
use App\Models\User;
use App\Models\Request;
use Illuminate\Database\Eloquent\Factories\Factory;

class UserRequestFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = UserRequest::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
           'user_id'=>User::factory(),
           'approvedBy_id'=>User::factory(),
            'request_id'=>Request::factory(),
            'assisted'=>$this->faker->boolean()
        ];
    }
}
